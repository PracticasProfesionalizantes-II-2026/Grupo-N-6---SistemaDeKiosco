using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Clases_KioPlus.Integraciones;

public interface IGeminiCliente
{
    // Le manda a Gemini un archivo junto con las instrucciones y devuelve el JSON
    // que respondió, forzado a respetar el esquema pedido.
    Task<ResultadoGemini> GenerarJsonAsync(
        string instrucciones, byte[] archivo, string tipoArchivo, JsonObject esquema);
}

public record ResultadoGemini(bool Ok, string? Json = null, string? Error = null)
{
    public static ResultadoGemini Exito(string json) => new(true, json);
    public static ResultadoGemini Fallo(string error) => new(false, null, error);
}

// Cliente HTTP de la API de Gemini (Google AI Studio). La clave se lee de la
// configuración "Gemini:ApiKey", que en desarrollo vive en user-secrets y nunca
// en el repositorio.
public class GeminiCliente : IGeminiCliente
{
    private const string UrlBase = "https://generativelanguage.googleapis.com/v1beta/models/";
    // En el plan gratuito gemini-3.8-flash tarda más de un minuto por pedido;
    // 3.5-flash lee una factura en pocos segundos con la misma precisión.
    private const string ModeloPorDefecto = "gemini-3.5-flash";
    private const int MaximoIntentos = 3;

    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<GeminiCliente> _logger;

    public GeminiCliente(HttpClient http, IConfiguration config, ILogger<GeminiCliente> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<ResultadoGemini> GenerarJsonAsync(
        string instrucciones, byte[] archivo, string tipoArchivo, JsonObject esquema)
    {
        var clave = _config["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(clave))
            return ResultadoGemini.Fallo("Falta configurar la clave de Gemini (Gemini:ApiKey) en la API.");

        var modelo = _config["Gemini:Modelo"] ?? ModeloPorDefecto;

        var cuerpo = new JsonObject
        {
            ["contents"] = new JsonArray
            {
                new JsonObject
                {
                    ["parts"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["inline_data"] = new JsonObject
                            {
                                ["mime_type"] = tipoArchivo,
                                ["data"] = Convert.ToBase64String(archivo)
                            }
                        },
                        new JsonObject { ["text"] = instrucciones }
                    }
                }
            },
            ["generationConfig"] = new JsonObject
            {
                ["responseMimeType"] = "application/json",
                ["responseSchema"] = esquema
            }
        };

        var json = cuerpo.ToJsonString();

        try
        {
            // En el plan gratuito Gemini a veces responde 503 (saturado) por unos
            // segundos: se reintenta un par de veces antes de avisarle al usuario.
            HttpResponseMessage respuesta;
            var intento = 1;
            while (true)
            {
                using var pedido = new HttpRequestMessage(HttpMethod.Post, $"{UrlBase}{modelo}:generateContent")
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
                pedido.Headers.Add("x-goog-api-key", clave);

                respuesta = await _http.SendAsync(pedido);
                if (respuesta.StatusCode != HttpStatusCode.ServiceUnavailable || intento == MaximoIntentos) break;

                respuesta.Dispose();
                await Task.Delay(TimeSpan.FromSeconds(2 * intento));
                intento++;
            }

            using var _ = respuesta;
            var texto = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gemini respondió {Estado}: {Cuerpo}", (int)respuesta.StatusCode, texto);
                return ResultadoGemini.Fallo(respuesta.StatusCode switch
                {
                    HttpStatusCode.TooManyRequests =>
                        "Se alcanzó el límite gratuito de Gemini. Esperá un minuto y volvé a intentar.",
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                        "La clave de Gemini no es válida o no tiene permisos.",
                    HttpStatusCode.ServiceUnavailable =>
                        "Gemini está saturado en este momento. Volvé a intentar en unos minutos.",
                    _ => $"Gemini no pudo procesar el archivo (error {(int)respuesta.StatusCode})."
                });
            }

            var resultado = ExtraerTexto(texto);
            return resultado is null
                ? ResultadoGemini.Fallo("Gemini no devolvió ninguna respuesta para este archivo.")
                : ResultadoGemini.Exito(resultado);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Falla al llamar a Gemini");
            return ResultadoGemini.Fallo("No se pudo conectar con Gemini. Revisá la conexión a internet.");
        }
    }

    // La respuesta trae candidates[0].content.parts; las partes marcadas como
    // "thought" son el razonamiento interno del modelo y se descartan.
    private static string? ExtraerTexto(string respuesta)
    {
        using var documento = JsonDocument.Parse(respuesta);
        if (!documento.RootElement.TryGetProperty("candidates", out var candidatos)
            || candidatos.GetArrayLength() == 0
            || !candidatos[0].TryGetProperty("content", out var contenido)
            || !contenido.TryGetProperty("parts", out var partes))
            return null;

        var textos = partes.EnumerateArray()
            .Where(p => !(p.TryGetProperty("thought", out var t) && t.ValueKind == JsonValueKind.True))
            .Where(p => p.TryGetProperty("text", out _))
            .Select(p => p.GetProperty("text").GetString());

        var texto = string.Concat(textos);
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }
}

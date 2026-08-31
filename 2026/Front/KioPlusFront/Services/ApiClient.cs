using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KioPlusFront.Services;

// Envoltorio único sobre HttpClient: centraliza la serialización, el manejo de
// errores de la API y la traducción de fallas de red a un mensaje legible.
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _logger;

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiClient(HttpClient http, ILogger<ApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<T>> ObtenerListaAsync<T>(string url)
    {
        var resultado = await ObtenerAsync<List<T>>(url);
        return resultado.Datos ?? new List<T>();
    }

    public async Task<ApiResultado<T>> ObtenerAsync<T>(string url)
    {
        try
        {
            var respuesta = await _http.GetAsync(url);
            if (respuesta.StatusCode == HttpStatusCode.NotFound)
                return ApiResultado<T>.Fallo("No se encontró el recurso solicitado.");

            if (!respuesta.IsSuccessStatusCode)
                return ApiResultado<T>.Fallo(await LeerErrorAsync(respuesta));

            var datos = await respuesta.Content.ReadFromJsonAsync<T>(Json);
            return ApiResultado<T>.Exito(datos);
        }
        catch (Exception ex)
        {
            return ApiResultado<T>.Fallo(DescribirFalla(ex, url));
        }
    }

    public Task<ApiResultado<TRespuesta>> PostAsync<TCuerpo, TRespuesta>(string url, TCuerpo cuerpo) =>
        EnviarConRespuestaAsync<TRespuesta>(HttpMethod.Post, url, cuerpo);

    public Task<ApiResultado> PostAsync<TCuerpo>(string url, TCuerpo cuerpo) =>
        EnviarAsync(HttpMethod.Post, url, cuerpo);

    public Task<ApiResultado> PutAsync<TCuerpo>(string url, TCuerpo cuerpo) =>
        EnviarAsync(HttpMethod.Put, url, cuerpo);

    public Task<ApiResultado> PatchAsync<TCuerpo>(string url, TCuerpo cuerpo) =>
        EnviarAsync(HttpMethod.Patch, url, cuerpo);

    public Task<ApiResultado> DeleteAsync(string url) =>
        EnviarAsync<object?>(HttpMethod.Delete, url, null);

    private async Task<ApiResultado> EnviarAsync<TCuerpo>(HttpMethod metodo, string url, TCuerpo cuerpo)
    {
        try
        {
            using var pedido = ArmarPedido(metodo, url, cuerpo);
            var respuesta = await _http.SendAsync(pedido);
            return respuesta.IsSuccessStatusCode
                ? ApiResultado.Exito()
                : ApiResultado.Fallo(await LeerErrorAsync(respuesta));
        }
        catch (Exception ex)
        {
            return ApiResultado.Fallo(DescribirFalla(ex, url));
        }
    }

    private async Task<ApiResultado<TRespuesta>> EnviarConRespuestaAsync<TRespuesta>(
        HttpMethod metodo, string url, object? cuerpo)
    {
        try
        {
            using var pedido = ArmarPedido(metodo, url, cuerpo);
            var respuesta = await _http.SendAsync(pedido);
            if (!respuesta.IsSuccessStatusCode)
                return ApiResultado<TRespuesta>.Fallo(await LeerErrorAsync(respuesta));

            var datos = await respuesta.Content.ReadFromJsonAsync<TRespuesta>(Json);
            return ApiResultado<TRespuesta>.Exito(datos);
        }
        catch (Exception ex)
        {
            return ApiResultado<TRespuesta>.Fallo(DescribirFalla(ex, url));
        }
    }

    private static HttpRequestMessage ArmarPedido(HttpMethod metodo, string url, object? cuerpo)
    {
        var pedido = new HttpRequestMessage(metodo, url);
        if (cuerpo is not null)
            pedido.Content = JsonContent.Create(cuerpo, cuerpo.GetType(), options: Json);
        return pedido;
    }

    // La API devuelve { mensaje, errores? }. Si no, se cae al texto crudo.
    private static async Task<string> LeerErrorAsync(HttpResponseMessage respuesta)
    {
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            return "Usuario o contraseña incorrectos.";

        var texto = await respuesta.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(texto))
            return $"La API respondió {(int)respuesta.StatusCode}.";

        try
        {
            using var documento = JsonDocument.Parse(texto);
            var raiz = documento.RootElement;

            var mensaje = raiz.TryGetProperty("mensaje", out var m) ? m.GetString() : null;

            if (raiz.TryGetProperty("errores", out var errores) && errores.ValueKind == JsonValueKind.Array)
            {
                var detalle = string.Join(" ", errores.EnumerateArray()
                    .Select(e => e.GetString())
                    .Where(e => !string.IsNullOrWhiteSpace(e)));
                if (!string.IsNullOrWhiteSpace(detalle))
                    return string.IsNullOrWhiteSpace(mensaje) ? detalle : $"{mensaje}: {detalle}";
            }

            return mensaje ?? $"La API respondió {(int)respuesta.StatusCode}.";
        }
        catch (JsonException)
        {
            return $"La API respondió {(int)respuesta.StatusCode}.";
        }
    }

    private string DescribirFalla(Exception ex, string url)
    {
        _logger.LogError(ex, "Falla al llamar a {Url}", url);
        return ex is HttpRequestException or TaskCanceledException
            ? "No se pudo conectar con el servidor. Verificá que la API esté levantada."
            : "Ocurrió un error inesperado al comunicarse con el servidor.";
    }
}

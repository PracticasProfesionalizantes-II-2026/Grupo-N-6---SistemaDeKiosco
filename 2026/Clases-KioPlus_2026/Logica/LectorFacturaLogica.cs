using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Clases_KioPlus.Integraciones;
using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;

// Lee una factura de proveedor en PDF con Gemini y la traduce a los datos del
// sistema. A la IA se le pasa el catálogo de productos y proveedores activos para
// que devuelva sus IDs; los IDs que no existan se descartan, así nunca se carga
// un producto inventado.
public class LectorFacturaLogica : ILectorFacturaLogica
{
    private readonly IGeminiCliente _gemini;
    private readonly IProductoRepositorio _productos;
    private readonly IProveedorRepositorio _proveedores;
    private readonly ILogger<LectorFacturaLogica> _logger;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public LectorFacturaLogica(
        IGeminiCliente gemini,
        IProductoRepositorio productos,
        IProveedorRepositorio proveedores,
        ILogger<LectorFacturaLogica> logger)
    {
        _gemini = gemini;
        _productos = productos;
        _proveedores = proveedores;
        _logger = logger;
    }

    public async Task<(FacturaInterpretadaDto? Factura, string? Error)> Interpretar(byte[] pdf)
    {
        var productos = (await _productos.ObtenerTodos(null, null, null)).ToList();
        var proveedores = (await _proveedores.ObtenerTodos(null)).ToList();

        var catalogo = new StringBuilder();
        catalogo.AppendLine("PROVEEDORES (id | razón social):");
        foreach (var p in proveedores) catalogo.AppendLine($"{p.Id} | {p.NombreRazonSocial}");
        catalogo.AppendLine();
        catalogo.AppendLine("PRODUCTOS (id | nombre | marca):");
        foreach (var p in productos) catalogo.AppendLine($"{p.Id} | {p.Nombre} | {p.Marca}");

        var instrucciones = $"""
            Sos un asistente que carga compras en el sistema de un kiosco.
            El archivo adjunto es una factura de un proveedor. Extraé:
            - El proveedor que emite la factura y su id en la lista de PROVEEDORES.
            - La fecha de emisión en formato yyyy-MM-dd.
            - Cada renglón de la factura: descripción tal cual figura, cantidad (unidades)
              y precio unitario (número, sin símbolo de moneda ni separador de miles).
            Para cada renglón buscá el producto equivalente en la lista de PRODUCTOS
            (puede estar escrito distinto, abreviado o con otra mayúscula) y poné su id.
            Si no hay un equivalente claro, dejá idProducto en null. Lo mismo con el proveedor.
            No inventes ids que no estén en las listas. Ignorá subtotales, impuestos y totales.

            {catalogo}
            """;

        var resultado = await _gemini.GenerarJsonAsync(instrucciones, pdf, "application/pdf", Esquema());
        if (!resultado.Ok) return (null, resultado.Error);

        RespuestaIa? respuesta;
        try
        {
            respuesta = JsonSerializer.Deserialize<RespuestaIa>(resultado.Json!, Json);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Gemini devolvió un JSON inválido: {Json}", resultado.Json);
            respuesta = null;
        }

        if (respuesta is null)
            return (null, "No se pudo interpretar la respuesta de la IA. Probá subir el archivo de nuevo.");

        var idsProductos = productos.Select(p => p.Id).ToHashSet();
        var idsProveedores = proveedores.Select(p => p.Id).ToHashSet();

        var renglones = (respuesta.Renglones ?? new())
            .Select(r => new RenglonFacturaDto(
                r.IdProducto is int id && idsProductos.Contains(id) ? id : null,
                r.Descripcion ?? string.Empty,
                r.Cantidad,
                r.PrecioUnitario))
            .ToList();

        var idProveedor = respuesta.IdProveedor is int idP && idsProveedores.Contains(idP) ? idP : (int?)null;

        DateTime? fecha = DateTime.TryParseExact(respuesta.Fecha, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var f) ? f : null;

        return (new FacturaInterpretadaDto(idProveedor, respuesta.Proveedor, fecha, renglones), null);
    }

    // Esquema que Gemini está obligado a respetar en su respuesta
    private static JsonObject Esquema() => new()
    {
        ["type"] = "OBJECT",
        ["properties"] = new JsonObject
        {
            ["idProveedor"] = new JsonObject { ["type"] = "INTEGER", ["nullable"] = true },
            ["proveedor"] = new JsonObject { ["type"] = "STRING" },
            ["fecha"] = new JsonObject { ["type"] = "STRING", ["description"] = "yyyy-MM-dd" },
            ["renglones"] = new JsonObject
            {
                ["type"] = "ARRAY",
                ["items"] = new JsonObject
                {
                    ["type"] = "OBJECT",
                    ["properties"] = new JsonObject
                    {
                        ["idProducto"] = new JsonObject { ["type"] = "INTEGER", ["nullable"] = true },
                        ["descripcion"] = new JsonObject { ["type"] = "STRING" },
                        ["cantidad"] = new JsonObject { ["type"] = "INTEGER" },
                        ["precioUnitario"] = new JsonObject { ["type"] = "NUMBER" }
                    },
                    ["required"] = new JsonArray("descripcion", "cantidad", "precioUnitario")
                }
            }
        },
        ["required"] = new JsonArray("proveedor", "renglones")
    };

    private class RespuestaIa
    {
        public int? IdProveedor { get; set; }
        public string? Proveedor { get; set; }
        public string? Fecha { get; set; }
        public List<RenglonIa>? Renglones { get; set; }
    }

    private class RenglonIa
    {
        public int? IdProducto { get; set; }
        public string? Descripcion { get; set; }
        public int Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
    }
}

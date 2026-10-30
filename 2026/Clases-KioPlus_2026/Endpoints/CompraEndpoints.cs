using Clases_KioPlus.Filters;
using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class CompraEndpoints
{
    public static void MapCompraEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/compras").WithTags("Compras");

        // Listado con nombre de proveedor y filtros opcionales
        grupo.MapGet("/", async (
            DateTime? fechaDesde, DateTime? fechaHasta, int? idProveedor,
            ICompraLogica logica) =>
            Results.Ok(await logica.ObtenerTodas(fechaDesde, fechaHasta, idProveedor)));

        grupo.MapGet("/{id:int}", async (int id, ICompraLogica logica) =>
        {
            var compra = await logica.ObtenerPorId(id);
            return compra is null ? Results.NotFound() : Results.Ok(compra);
        });

        grupo.MapPost("/", async (CompraCreateDto dto, ICompraLogica logica) =>
        {
            var id = await logica.Crear(dto);
            return id is null
                ? Results.NotFound(new { mensaje = "proveedor no encontrado" })
                : Results.Created($"/compras/{id}", new { idCompraProveedor = id, idProveedor = dto.IdProveedor });
        }).AddEndpointFilter<ValidationFilter<CompraCreateDto>>();

        // Lee una factura en PDF con IA y devuelve un borrador de la compra.
        // No registra nada: la compra la confirma el usuario desde el front.
        // DisableAntiforgery: es una API JSON sin cookies; el front ya valida su token.
        grupo.MapPost("/interpretar-factura", async (IFormFile archivo, ILectorFacturaLogica logica) =>
        {
            if (archivo.Length == 0)
                return Results.BadRequest(new { mensaje = "El archivo está vacío." });

            if (!string.Equals(Path.GetExtension(archivo.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest(new { mensaje = "El archivo tiene que ser un PDF." });

            // Gemini acepta hasta 20 MB por pedido con el archivo embebido
            if (archivo.Length > 15 * 1024 * 1024)
                return Results.BadRequest(new { mensaje = "El PDF no puede superar los 15 MB." });

            using var memoria = new MemoryStream();
            await archivo.CopyToAsync(memoria);

            var (factura, error) = await logica.Interpretar(memoria.ToArray());
            return factura is null
                ? Results.UnprocessableEntity(new { mensaje = error })
                : Results.Ok(factura);
        }).DisableAntiforgery();

        grupo.MapPut("/{id:int}", async (int id, CompraCreateDto dto, ICompraLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "compra actualizada" }) : Results.NotFound();
        }).AddEndpointFilter<ValidationFilter<CompraCreateDto>>();

        grupo.MapDelete("/{id:int}", async (int id, ICompraLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.Ok(new { mensaje = "compra eliminada" }) : Results.NotFound();
        });
    }
}

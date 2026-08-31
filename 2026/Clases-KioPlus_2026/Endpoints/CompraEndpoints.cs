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

using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class DetalleVentaEndpoints
{
    public static void MapDetalleVentaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/ventas/{idVenta:int}/detalles").WithTags("DetalleVenta");

        grupo.MapGet("/", async (int idVenta, IDetalleVentaLogica logica) =>
            Results.Ok(await logica.ObtenerPorVenta(idVenta)));

        grupo.MapGet("/{id:int}", async (int idVenta, int id, IDetalleVentaLogica logica) =>
        {
            var detalle = await logica.ObtenerPorId(id);
            return detalle is null ? Results.NotFound() : Results.Ok(detalle);
        });

        grupo.MapPost("/", async (int idVenta, DetalleVentaCreateDto dto, IDetalleVentaLogica logica) =>
        {
            var id = await logica.Crear(idVenta, dto);
            return id is null
                ? Results.NotFound(new { mensaje = "venta o producto no encontrados" })
                : Results.Created($"/ventas/{idVenta}/detalles/{id}", new { idDetalle = id });
        });

        grupo.MapPut("/{id:int}", async (int idVenta, int id, DetalleVentaUpdateDto dto, IDetalleVentaLogica logica) =>
        {
            var ok = await logica.Actualizar(idVenta, id, dto);
            return ok ? Results.Ok(new { mensaje = "detalle de venta actualizado" }) : Results.NotFound();
        });

        grupo.MapDelete("/{id:int}", async (int idVenta, int id, IDetalleVentaLogica logica) =>
        {
            var ok = await logica.Eliminar(idVenta, id);
            return ok ? Results.Ok(new { mensaje = "detalle de venta eliminado" }) : Results.NotFound();
        });
    }
}

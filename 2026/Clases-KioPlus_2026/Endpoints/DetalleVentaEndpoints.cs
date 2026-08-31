using Clases_KioPlus.Filters;
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
            return detalle is null || detalle.IdVenta != idVenta
                ? Results.NotFound()
                : Results.Ok(detalle);
        });

        // Alta del renglón: descuenta stock y devuelve 400 si no alcanza
        grupo.MapPost("/", async (int idVenta, DetalleVentaCreateDto dto, IDetalleVentaLogica logica) =>
        {
            var resultado = await logica.Crear(idVenta, dto);
            if (resultado.Ok)
                return Results.Created($"/ventas/{idVenta}/detalles/{resultado.Id}", new { idDetalle = resultado.Id });

            return resultado.Error!.Contains("no encontrad")
                ? Results.NotFound(new { mensaje = resultado.Error })
                : Results.BadRequest(new { mensaje = resultado.Error });
        }).AddEndpointFilter<ValidationFilter<DetalleVentaCreateDto>>();

        grupo.MapPut("/{id:int}", async (int idVenta, int id, DetalleVentaUpdateDto dto, IDetalleVentaLogica logica) =>
        {
            var resultado = await logica.Actualizar(idVenta, id, dto);
            if (resultado.Ok) return Results.Ok(new { mensaje = "detalle actualizado" });

            return resultado.Error!.Contains("no encontrad")
                ? Results.NotFound(new { mensaje = resultado.Error })
                : Results.BadRequest(new { mensaje = resultado.Error });
        }).AddEndpointFilter<ValidationFilter<DetalleVentaUpdateDto>>();

        grupo.MapDelete("/{id:int}", async (int idVenta, int id, IDetalleVentaLogica logica) =>
        {
            var ok = await logica.Eliminar(idVenta, id);
            return ok ? Results.Ok(new { mensaje = "detalle eliminado" }) : Results.NotFound();
        });
    }
}

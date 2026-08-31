using Clases_KioPlus.Filters;
using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class DetalleCompraEndpoints
{
    public static void MapDetalleCompraEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/compras/{idCompra:int}/detalles", async (int idCompra, IDetalleCompraLogica logica) =>
            Results.Ok(await logica.ObtenerPorCompra(idCompra))).WithTags("DetalleCompra");

        app.MapGet("/compras/{idCompra:int}/detalles/{id:int}", async (int idCompra, int id, IDetalleCompraLogica logica) =>
        {
            var detalle = await logica.ObtenerPorId(id);
            return detalle is null || detalle.IdCompra != idCompra
                ? Results.NotFound()
                : Results.Ok(detalle);
        }).WithTags("DetalleCompra");

        // Alta del renglón: ingresa la mercadería al stock del producto
        app.MapPost("/compras/{idCompra:int}/detalles", async (int idCompra, DetalleCompraCreateDto dto, IDetalleCompraLogica logica) =>
        {
            var resultado = await logica.Crear(idCompra, dto);
            if (resultado.Ok)
                return Results.Created($"/compras/{idCompra}/detalles/{resultado.Id}", new { idDetalleCompra = resultado.Id });

            return resultado.Error!.Contains("no encontrad")
                ? Results.NotFound(new { mensaje = resultado.Error })
                : Results.BadRequest(new { mensaje = resultado.Error });
        }).AddEndpointFilter<ValidationFilter<DetalleCompraCreateDto>>().WithTags("DetalleCompra");

        app.MapPut("/detallecompras/{id:int}", async (int id, DetalleCompraUpdateDto dto, IDetalleCompraLogica logica) =>
        {
            var resultado = await logica.Actualizar(id, dto);
            if (resultado.Ok) return Results.Ok(new { mensaje = "detalle actualizado" });

            return resultado.Error!.Contains("no encontrad")
                ? Results.NotFound(new { mensaje = resultado.Error })
                : Results.BadRequest(new { mensaje = resultado.Error });
        }).AddEndpointFilter<ValidationFilter<DetalleCompraUpdateDto>>().WithTags("DetalleCompra");

        app.MapDelete("/detallecompras/{id:int}", async (int id, IDetalleCompraLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.Ok(new { mensaje = "detalle eliminado" }) : Results.NotFound();
        }).WithTags("DetalleCompra");
    }
}

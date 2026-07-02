using Clases_KioPlus.Filters;
using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class DetalleCompraEndpoints
{
    public static void MapDetalleCompraEndpoints(this IEndpointRouteBuilder app)
    {
        // Detalles anidados bajo una compra
        app.MapGet("/compras/{idCompra:int}/detalles", async (int idCompra, IDetalleCompraLogica logica) =>
            Results.Ok(await logica.ObtenerPorCompra(idCompra)))
            .WithTags("DetalleCompra");

        app.MapGet("/compras/{idCompra:int}/detalles/{id:int}", async (int idCompra, int id, IDetalleCompraLogica logica) =>
        {
            var detalle = await logica.ObtenerPorId(id);
            return detalle is null ? Results.NotFound() : Results.Ok(detalle);
        }).WithTags("DetalleCompra");

        app.MapPost("/compras/{idCompra:int}/detalles", async (int idCompra, DetalleCompraCreateDto dto, IDetalleCompraLogica logica) =>
        {
            var id = await logica.Crear(idCompra, dto);
            return id is null
                ? Results.NotFound(new { mensaje = "compra no encontrada" })
                : Results.Created($"/compras/{idCompra}/detalles/{id}", new { idDetalleCompra = id });
        }).WithTags("DetalleCompra").AddEndpointFilter<ValidationFilter<DetalleCompraCreateDto>>();

        // Editar / eliminar detalle de compra directamente por su id
        app.MapPut("/detallecompras/{id:int}", async (int id, DetalleCompraUpdateDto dto, IDetalleCompraLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "detalle compra actualizado" }) : Results.NotFound();
        }).WithTags("DetalleCompra").AddEndpointFilter<ValidationFilter<DetalleCompraUpdateDto>>();

        app.MapDelete("/detallecompras/{id:int}", async (int id, IDetalleCompraLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.NoContent() : Results.NotFound();
        }).WithTags("DetalleCompra");
    }
}

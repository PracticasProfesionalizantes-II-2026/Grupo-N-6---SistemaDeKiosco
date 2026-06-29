using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class LoteEndpoints
{
    public static void MapLoteEndpoints(this IEndpointRouteBuilder app)
    {
        // Lotes de un producto
        app.MapGet("/productos/{idProducto:int}/lotes", async (int idProducto, ILoteLogica logica) =>
            Results.Ok(await logica.ObtenerPorProducto(idProducto)))
            .WithTags("Lotes");

        app.MapGet("/productos/{idProducto:int}/lotes/{id:int}", async (int idProducto, int id, ILoteLogica logica) =>
        {
            var lote = await logica.ObtenerPorId(id);
            return lote is null ? Results.NotFound() : Results.Ok(lote);
        }).WithTags("Lotes");

        app.MapPost("/productos/{idProducto:int}/lotes", async (int idProducto, LoteCreateDto dto, ILoteLogica logica) =>
        {
            var id = await logica.Crear(idProducto, dto);
            return id is null
                ? Results.NotFound(new { mensaje = "producto no encontrado" })
                : Results.Created($"/productos/{idProducto}/lotes/{id}", new { idLote = id });
        }).WithTags("Lotes");

        // Editar / eliminar lote directamente por su id
        app.MapPut("/lotes/{id:int}", async (int id, LoteCreateDto dto, ILoteLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "lote actualizado" }) : Results.NotFound();
        }).WithTags("Lotes");

        app.MapDelete("/lotes/{id:int}", async (int id, ILoteLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.Ok(new { mensaje = "lote eliminado" }) : Results.NotFound();
        }).WithTags("Lotes");
    }
}

using Clases_KioPlus.Filters;
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
            var resultado = await logica.Crear(idProducto, dto);
            if (resultado.Ok)
                return Results.Created($"/productos/{idProducto}/lotes/{resultado.Id}", new { idLote = resultado.Id });

            return resultado.Error!.Contains("no encontrad")
                ? Results.NotFound(new { mensaje = resultado.Error })
                : Results.BadRequest(new { mensaje = resultado.Error });
        }).WithTags("Lotes").AddEndpointFilter<ValidationFilter<LoteCreateDto>>();

        // Editar / eliminar lote directamente por su id
        app.MapPut("/lotes/{id:int}", async (int id, LoteCreateDto dto, ILoteLogica logica) =>
        {
            var resultado = await logica.Actualizar(id, dto);
            if (resultado.Ok) return Results.Ok(new { mensaje = "lote actualizado" });

            return resultado.Error!.Contains("no encontrad")
                ? Results.NotFound(new { mensaje = resultado.Error })
                : Results.BadRequest(new { mensaje = resultado.Error });
        }).WithTags("Lotes").AddEndpointFilter<ValidationFilter<LoteCreateDto>>();

        app.MapDelete("/lotes/{id:int}", async (int id, ILoteLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.NoContent() : Results.NotFound();
        }).WithTags("Lotes");
    }
}

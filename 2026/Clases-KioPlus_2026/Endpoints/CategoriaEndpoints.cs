using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class CategoriaEndpoints
{
    public static void MapCategoriaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/categorias").WithTags("Categorias");

        grupo.MapGet("/", async (ICategoriaLogica logica) =>
            Results.Ok(await logica.ObtenerTodas()));

        grupo.MapGet("/{id:int}", async (int id, ICategoriaLogica logica) =>
        {
            var categoria = await logica.ObtenerPorId(id);
            return categoria is null ? Results.NotFound() : Results.Ok(categoria);
        });

        grupo.MapPost("/", async (CategoriaCreateDto dto, ICategoriaLogica logica) =>
        {
            var id = await logica.Crear(dto);
            return Results.Created($"/categorias/{id}", new { idCategoria = id });
        });

        grupo.MapPut("/{id:int}", async (int id, CategoriaCreateDto dto, ICategoriaLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "categoría actualizada" }) : Results.NotFound();
        });

        grupo.MapDelete("/{id:int}", async (int id, ICategoriaLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.Ok(new { mensaje = "categoría eliminada" }) : Results.NotFound();
        });
    }
}

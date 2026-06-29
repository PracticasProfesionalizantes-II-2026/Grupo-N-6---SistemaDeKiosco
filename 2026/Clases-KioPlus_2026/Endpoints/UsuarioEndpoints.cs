using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/usuarios").WithTags("Usuarios");

        grupo.MapGet("/", async (IUsuarioLogica logica) =>
            Results.Ok(await logica.ObtenerTodos()));

        grupo.MapGet("/{id:int}", async (int id, IUsuarioLogica logica) =>
        {
            var usuario = await logica.ObtenerPorId(id);
            return usuario is null ? Results.NotFound() : Results.Ok(usuario);
        });

        grupo.MapPost("/", async (UsuarioCreateDto dto, IUsuarioLogica logica) =>
        {
            var id = await logica.Crear(dto);
            return Results.Created($"/usuarios/{id}", new { idUsuario = id });
        });

        grupo.MapPut("/{id:int}", async (int id, UsuarioCreateDto dto, IUsuarioLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "usuario actualizado" }) : Results.NotFound();
        });

        grupo.MapDelete("/{id:int}", async (int id, IUsuarioLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.Ok(new { mensaje = "usuario eliminado" }) : Results.NotFound();
        });
    }
}

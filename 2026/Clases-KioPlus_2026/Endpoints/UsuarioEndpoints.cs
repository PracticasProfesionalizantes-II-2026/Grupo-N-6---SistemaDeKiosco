using Clases_KioPlus.Filters;
using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/usuarios").WithTags("Usuarios");

        // Inicio de sesión. 401 si las credenciales no coinciden o el usuario está bloqueado.
        grupo.MapPost("/login", async (LoginDto dto, IUsuarioLogica logica) =>
        {
            var sesion = await logica.Login(dto);
            return sesion is null
                ? Results.Unauthorized()
                : Results.Ok(sesion);
        }).AddEndpointFilter<ValidationFilter<LoginDto>>();

        grupo.MapGet("/", async (IUsuarioLogica logica) =>
            Results.Ok(await logica.ObtenerTodos()));

        grupo.MapGet("/{id:int}", async (int id, IUsuarioLogica logica) =>
        {
            var usuario = await logica.ObtenerPorId(id);
            return usuario is null ? Results.NotFound() : Results.Ok(usuario);
        });

        grupo.MapPost("/", async (UsuarioCreateDto dto, IUsuarioLogica logica) =>
        {
            var resultado = await logica.Crear(dto);
            return resultado.Ok
                ? Results.Created($"/usuarios/{resultado.Id}", new { idUsuario = resultado.Id })
                : Results.BadRequest(new { mensaje = resultado.Error });
        }).AddEndpointFilter<ValidationFilter<UsuarioCreateDto>>();

        grupo.MapPut("/{id:int}", async (int id, UsuarioCreateDto dto, IUsuarioLogica logica) =>
        {
            var resultado = await logica.Actualizar(id, dto);
            if (resultado.Ok) return Results.Ok(new { mensaje = "usuario actualizado" });
            return resultado.Error == "usuario no encontrado"
                ? Results.NotFound()
                : Results.BadRequest(new { mensaje = resultado.Error });
        }).AddEndpointFilter<ValidationFilter<UsuarioCreateDto>>();

        // Bloqueo / desbloqueo del acceso (candado del listado)
        grupo.MapPatch("/{id:int}/estado", async (int id, CambiarEstadoUsuarioDto dto, IUsuarioLogica logica) =>
        {
            var ok = await logica.CambiarEstado(id, dto.Estado);
            return ok ? Results.Ok(new { mensaje = "estado actualizado" }) : Results.NotFound();
        });

        grupo.MapDelete("/{id:int}", async (int id, IUsuarioLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.Ok(new { mensaje = "usuario eliminado" }) : Results.NotFound();
        });
    }
}

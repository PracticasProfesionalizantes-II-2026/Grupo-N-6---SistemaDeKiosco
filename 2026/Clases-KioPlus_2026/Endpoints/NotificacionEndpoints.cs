using Clases_KioPlus.Logica;

namespace Clases_KioPlus.Endpoints;

public static class NotificacionEndpoints
{
    public static void MapNotificacionEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/notificaciones").WithTags("Notificaciones");

        // Listar todas, o filtrar con ?tipo=stock | ?tipo=vencimiento
        grupo.MapGet("/", async (string? tipo, INotificacionLogica logica) =>
            Results.Ok(await logica.ObtenerTodas(tipo)));
    }
}

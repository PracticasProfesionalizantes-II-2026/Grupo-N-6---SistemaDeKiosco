using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;

public interface INotificacionLogica
{
    Task<IEnumerable<NotificacionDto>> ObtenerTodas(string? tipo);
}

public class NotificacionLogica : INotificacionLogica
{
    private readonly INotificacionRepositorio _repo;
    public NotificacionLogica(INotificacionRepositorio repo) => _repo = repo;

    public async Task<IEnumerable<NotificacionDto>> ObtenerTodas(string? tipo)
    {
        Notificacion.TipoNotificacion? tipoEnum = tipo?.ToLower() switch
        {
            "stock" => Notificacion.TipoNotificacion.StockBajo,
            "vencimiento" => Notificacion.TipoNotificacion.ProximoVencimiento,
            _ => null
        };

        var notificaciones = await _repo.ObtenerTodas(tipoEnum);
        return notificaciones.Select(n => new NotificacionDto(n.Id, n.Tipo, n.Mensaje, n.FechaGeneracion));
    }
}

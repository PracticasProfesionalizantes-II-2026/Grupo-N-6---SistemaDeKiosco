using Clases_KioPlus.Models;

namespace Clases_KioPlus.Logica.DTOs;

public record NotificacionDto(
    int Id,
    Notificacion.TipoNotificacion Tipo,
    string Mensaje,
    DateTime FechaGeneracion);

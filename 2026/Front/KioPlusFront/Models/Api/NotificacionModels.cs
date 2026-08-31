namespace KioPlusFront.Models.Api;

public enum TipoNotificacion
{
    StockBajo,
    ProximoVencimiento
}

public record NotificacionDto(
    int Id,
    TipoNotificacion Tipo,
    string Mensaje,
    DateTime FechaGeneracion);

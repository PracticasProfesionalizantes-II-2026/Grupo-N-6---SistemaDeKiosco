using KioPlusFront.Models.Api;

namespace KioPlusFront.Services;

public interface INotificacionService
{
    Task<IReadOnlyList<NotificacionDto>> ObtenerTodasAsync(string? tipo = null);
}

public class NotificacionService : INotificacionService
{
    private readonly ApiClient _api;
    public NotificacionService(ApiClient api) => _api = api;

    public Task<IReadOnlyList<NotificacionDto>> ObtenerTodasAsync(string? tipo = null)
    {
        var query = string.IsNullOrWhiteSpace(tipo) ? string.Empty : $"?tipo={tipo}";
        return _api.ObtenerListaAsync<NotificacionDto>($"/notificaciones{query}");
    }
}

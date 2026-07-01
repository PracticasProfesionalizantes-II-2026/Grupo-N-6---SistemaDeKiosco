using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface INotificacionLogica
{
    Task<IEnumerable<NotificacionDto>> ObtenerTodas(string? tipo);
}
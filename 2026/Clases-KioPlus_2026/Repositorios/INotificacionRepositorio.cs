using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface INotificacionRepositorio
{
    Task<IEnumerable<Notificacion>> ObtenerTodas(Notificacion.TipoNotificacion? tipo);
}

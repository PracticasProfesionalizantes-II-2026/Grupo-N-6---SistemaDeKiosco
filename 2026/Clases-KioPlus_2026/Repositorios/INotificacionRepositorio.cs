using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface INotificacionRepositorio
{
    Task<IEnumerable<Notificacion>> ObtenerTodas(Notificacion.TipoNotificacion? tipo);
    Task<IEnumerable<Producto>> ObtenerProductosConStockCritico(int umbral);
    Task<IEnumerable<Lote>> ObtenerLotesConProducto();
    // Reemplaza el tablero de avisos por el estado actual del stock
    Task Regenerar(IEnumerable<Notificacion> notificaciones);
}

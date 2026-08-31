using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public class NotificacionRepositorio : INotificacionRepositorio
{
    private readonly ApplicationDbContext _db;
    public NotificacionRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Notificacion>> ObtenerTodas(Notificacion.TipoNotificacion? tipo)
    {
        var query = _db.Notificaciones.AsQueryable();
        if (tipo.HasValue)
            query = query.Where(n => n.Tipo == tipo.Value);
        return await query.OrderByDescending(n => n.FechaGeneracion).ToListAsync();
    }

    public async Task<IEnumerable<Producto>> ObtenerProductosConStockCritico(int umbral) =>
        await _db.Productos.Where(p => p.StockDisponible <= umbral).ToListAsync();

    public async Task<IEnumerable<Lote>> ObtenerLotesConProducto() =>
        await _db.Lotes.Include(l => l.Producto).ToListAsync();

    // Los avisos son un reflejo del estado actual: se recalculan enteros en cada consulta
    // para que no queden alertas viejas de productos que ya se repusieron.
    public async Task Regenerar(IEnumerable<Notificacion> notificaciones)
    {
        _db.Notificaciones.RemoveRange(_db.Notificaciones);
        await _db.Notificaciones.AddRangeAsync(notificaciones);
        await _db.SaveChangesAsync();
    }
}

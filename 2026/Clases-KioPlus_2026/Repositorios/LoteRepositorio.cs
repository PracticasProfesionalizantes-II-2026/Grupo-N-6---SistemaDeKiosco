using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

// Único punto por el que entra y sale mercadería: el stock del producto siempre
// queda como la suma de sus lotes.
public class LoteRepositorio : ILoteRepositorio
{
    private readonly ApplicationDbContext _db;
    public LoteRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<bool> ProductoExiste(int idProducto) =>
        await _db.Productos.AnyAsync(p => p.Id == idProducto);

    public async Task<IEnumerable<Lote>> ObtenerPorProducto(int idProducto) =>
        await OrdenadosFefo(idProducto).ToListAsync();

    public async Task<Lote?> ObtenerPorId(int id) =>
        await _db.Lotes.FindAsync(id);

    public async Task<Lote> Agregar(Lote lote)
    {
        _db.Lotes.Add(lote);
        await _db.SaveChangesAsync();
        return lote;
    }

    public async Task Actualizar(Lote lote)
    {
        _db.Lotes.Update(lote);
        await _db.SaveChangesAsync();
    }

    public async Task Eliminar(Lote lote)
    {
        _db.Lotes.Remove(lote);
        await _db.SaveChangesAsync();
    }

    public async Task<Lote?> ObtenerPorDetalleCompra(int idDetalleCompra) =>
        await _db.Lotes.FirstOrDefaultAsync(l => l.DetalleCompraId == idDetalleCompra);

    public async Task EliminarPorDetallesCompra(IEnumerable<int> idsDetalleCompra)
    {
        var ids = idsDetalleCompra.ToList();
        if (ids.Count == 0) return;

        var lotes = await _db.Lotes
            .Where(l => l.DetalleCompraId != null && ids.Contains(l.DetalleCompraId.Value))
            .ToListAsync();
        if (lotes.Count == 0) return;

        _db.Lotes.RemoveRange(lotes);
        await _db.SaveChangesAsync();
    }

    public async Task RecalcularStockProducto(int idProducto)
    {
        var producto = await _db.Productos.FindAsync(idProducto);
        if (producto is null) return;

        producto.StockDisponible = await _db.Lotes
            .Where(l => l.ProductoId == idProducto)
            .SumAsync(l => (int?)l.Cantidad) ?? 0;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ConsumirFefo(int idProducto, int cantidad)
    {
        if (cantidad <= 0) return true;

        var lotes = await OrdenadosFefo(idProducto).Where(l => l.Cantidad > 0).ToListAsync();
        if (lotes.Sum(l => l.Cantidad) < cantidad) return false;

        var restante = cantidad;
        foreach (var lote in lotes)
        {
            if (restante == 0) break;

            var tomado = Math.Min(lote.Cantidad, restante);
            lote.Cantidad -= tomado;
            restante -= tomado;
        }

        await _db.SaveChangesAsync();
        await RecalcularStockProducto(idProducto);
        return true;
    }

    // Si las unidades a devolver no entran en ningún lote (porque los lotes se
    // borraron después de la venta) el sobrante se descarta: el stock no puede
    // ser mayor que la mercadería efectivamente registrada.
    public async Task DevolverFefo(int idProducto, int cantidad)
    {
        if (cantidad <= 0) return;

        var lotes = await OrdenadosFefo(idProducto).ToListAsync();
        var restante = cantidad;

        foreach (var lote in lotes)
        {
            if (restante == 0) break;

            var hueco = lote.CantidadInicial - lote.Cantidad;
            if (hueco <= 0) continue;

            var repuesto = Math.Min(hueco, restante);
            lote.Cantidad += repuesto;
            restante -= repuesto;
        }

        await _db.SaveChangesAsync();
        await RecalcularStockProducto(idProducto);
    }

    // Orden de consumo: primero lo que vence antes; los lotes sin vencimiento, últimos.
    private IQueryable<Lote> OrdenadosFefo(int idProducto) =>
        _db.Lotes
            .Where(l => l.ProductoId == idProducto)
            .OrderBy(l => l.FechaVencimiento == null)
            .ThenBy(l => l.FechaVencimiento)
            .ThenBy(l => l.Id);
}

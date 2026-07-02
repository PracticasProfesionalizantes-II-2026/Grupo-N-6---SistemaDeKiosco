using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public class ProductoRepositorio : IProductoRepositorio
{
    private readonly ApplicationDbContext _db;
    public ProductoRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Producto>> ObtenerTodos(string? nombre, int? idCategoria, string? marca)
    {
        var query = _db.Productos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(p => p.Nombre.Contains(nombre));
        if (idCategoria.HasValue)
            query = query.Where(p => p.CategoriaId == idCategoria.Value);
        if (!string.IsNullOrWhiteSpace(marca))
            query = query.Where(p => p.Marca.Contains(marca));
        return await query.ToListAsync();
    }

    public async Task<Producto?> ObtenerPorId(int id) =>
        await _db.Productos.FindAsync(id);

    public async Task<Producto?> ObtenerConLotes(int id) =>
        await _db.Productos.Include(p => p.Lotes).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Producto> Agregar(Producto producto)
    {
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();
        return producto;
    }

    public async Task Actualizar(Producto producto)
    {
        _db.Productos.Update(producto);
        await _db.SaveChangesAsync();
    }

    public async Task Eliminar(Producto producto)
    {
        _db.Productos.Remove(producto);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Producto>> ObtenerConStockCritico(int umbral) =>
        await _db.Productos.Where(p => p.StockDisponible <= umbral).ToListAsync();

    public async Task<IEnumerable<Lote>> ObtenerLotesConProducto() =>
        await _db.Lotes.Include(l => l.Producto).ToListAsync();

    public async Task<IEnumerable<(string Nombre, int Cantidad, double Monto)>> MasVendidos(DateTime desde, DateTime hasta)
    {
        var resultado = await (
            from d in _db.DetallesVentas
            join v in _db.Ventas on d.VentaId equals v.Id
            join p in _db.Productos on d.ProductoId equals p.Id
            where v.FechaHora >= desde && v.FechaHora <= hasta
            group d by new { p.Id, p.Nombre } into g
            select new
            {
                g.Key.Nombre,
                Cantidad = g.Sum(x => x.Cantidad),
                Monto = g.Sum(x => x.Subtotal)
            }).ToListAsync();

        return resultado.Select(r => (r.Nombre, r.Cantidad, r.Monto));
    }
}

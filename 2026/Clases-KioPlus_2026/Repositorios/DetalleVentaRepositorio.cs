using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public class DetalleVentaRepositorio : IDetalleVentaRepositorio
{
    private readonly ApplicationDbContext _db;
    public DetalleVentaRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<bool> VentaExiste(int idVenta) =>
        await _db.Ventas.AnyAsync(v => v.Id == idVenta);

    public async Task<Producto?> ObtenerProducto(int idProducto) =>
        await _db.Productos.FindAsync(idProducto);

    public async Task<IEnumerable<DetalleVenta>> ObtenerPorVenta(int idVenta) =>
        await _db.DetallesVentas.Where(d => d.VentaId == idVenta).ToListAsync();

    // Trae el detalle junto al nombre del producto para el listado del front
    public async Task<IEnumerable<(DetalleVenta Detalle, string Producto)>> ObtenerPorVentaConProducto(int idVenta)
    {
        var filas = await (
            from d in _db.DetallesVentas
            join p in _db.Productos on d.ProductoId equals p.Id into gp
            from p in gp.DefaultIfEmpty()
            where d.VentaId == idVenta
            select new { Detalle = d, Nombre = p != null ? p.Nombre : "(producto eliminado)" }
        ).ToListAsync();

        return filas.Select(f => (f.Detalle, f.Nombre));
    }

    public async Task<DetalleVenta?> ObtenerPorId(int id) =>
        await _db.DetallesVentas.FindAsync(id);

    public async Task<DetalleVenta> Agregar(DetalleVenta detalle)
    {
        _db.DetallesVentas.Add(detalle);
        await _db.SaveChangesAsync();
        return detalle;
    }

    public async Task Actualizar(DetalleVenta detalle)
    {
        _db.DetallesVentas.Update(detalle);
        await _db.SaveChangesAsync();
    }

    public async Task Eliminar(DetalleVenta detalle)
    {
        _db.DetallesVentas.Remove(detalle);
        await _db.SaveChangesAsync();
    }

    // Recalcula el monto total de la venta a partir de la suma de sus detalles
    public async Task RecalcularMontoVenta(int idVenta)
    {
        var venta = await _db.Ventas.FindAsync(idVenta);
        if (venta is null) return;

        venta.MontoTotal = await _db.DetallesVentas
            .Where(d => d.VentaId == idVenta)
            .SumAsync(d => d.Subtotal);
        await _db.SaveChangesAsync();
    }

    // Mueve el stock disponible del producto. Nunca lo deja por debajo de cero.
    public async Task AjustarStock(int idProducto, int delta)
    {
        var producto = await _db.Productos.FindAsync(idProducto);
        if (producto is null) return;

        producto.StockDisponible = Math.Max(0, producto.StockDisponible + delta);
        await _db.SaveChangesAsync();
    }
}

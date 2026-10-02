using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public class DetalleCompraRepositorio : IDetalleCompraRepositorio
{
    private readonly ApplicationDbContext _db;
    public DetalleCompraRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<bool> CompraExiste(int idCompra) =>
        await _db.Compras.AnyAsync(c => c.Id == idCompra);

    public async Task<int?> ObtenerProveedorDeCompra(int idCompra)
    {
        var compra = await _db.Compras.FindAsync(idCompra);
        return compra?.ProveedorId;
    }

    public async Task<Producto?> ObtenerProducto(int idProducto) =>
        await _db.Productos.FindAsync(idProducto);

    public async Task<IEnumerable<DetalleCompra>> ObtenerPorCompra(int idCompra) =>
        await _db.DetallesCompras.Where(d => d.CompraProveedorId == idCompra).ToListAsync();

    // Trae el detalle junto al nombre del producto y al lote que ingresó, para el listado del front
    public async Task<IEnumerable<(DetalleCompra Detalle, string Producto, Lote? Lote)>> ObtenerPorCompraConProducto(int idCompra)
    {
        var filas = await (
            from d in _db.DetallesCompras
            join p in _db.Productos on d.ProductoId equals p.Id into gp
            from p in gp.DefaultIfEmpty()
            join l in _db.Lotes on d.Id equals l.DetalleCompraId into gl
            from l in gl.DefaultIfEmpty()
            where d.CompraProveedorId == idCompra
            select new { Detalle = d, Nombre = p != null ? p.Nombre : "(producto eliminado)", Lote = l }
        ).ToListAsync();

        return filas.Select(f => (f.Detalle, f.Nombre, (Lote?)f.Lote));
    }

    public async Task<DetalleCompra?> ObtenerPorId(int id) =>
        await _db.DetallesCompras.FindAsync(id);

    public async Task<DetalleCompra> Agregar(DetalleCompra detalle)
    {
        _db.DetallesCompras.Add(detalle);
        await _db.SaveChangesAsync();
        return detalle;
    }

    public async Task Actualizar(DetalleCompra detalle)
    {
        _db.DetallesCompras.Update(detalle);
        await _db.SaveChangesAsync();
    }

    public async Task Eliminar(DetalleCompra detalle)
    {
        _db.DetallesCompras.Remove(detalle);
        await _db.SaveChangesAsync();
    }

    // Recalcula el monto total de la compra a partir de la suma de sus detalles
    public async Task RecalcularMontoCompra(int idCompra)
    {
        var compra = await _db.Compras.FindAsync(idCompra);
        if (compra is null) return;

        compra.MontoTotal = await _db.DetallesCompras
            .Where(d => d.CompraProveedorId == idCompra)
            .SumAsync(d => d.Subtotal);
        await _db.SaveChangesAsync();
    }
}

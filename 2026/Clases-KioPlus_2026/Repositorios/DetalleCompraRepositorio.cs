using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface IDetalleCompraRepositorio
{
    Task<bool> CompraExiste(int idCompra);
    Task<IEnumerable<DetalleCompra>> ObtenerPorCompra(int idCompra);
    Task<DetalleCompra?> ObtenerPorId(int id);
    Task<DetalleCompra> Agregar(DetalleCompra detalle);
    Task Actualizar(DetalleCompra detalle);
    Task Eliminar(DetalleCompra detalle);
    Task RecalcularMontoCompra(int idCompra);
}

public class DetalleCompraRepositorio : IDetalleCompraRepositorio
{
    private readonly ApplicationDbContext _db;
    public DetalleCompraRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<bool> CompraExiste(int idCompra) =>
        await _db.Compras.AnyAsync(c => c.Id == idCompra);

    public async Task<IEnumerable<DetalleCompra>> ObtenerPorCompra(int idCompra) =>
        await _db.DetallesCompras.Where(d => d.CompraProveedorId == idCompra).ToListAsync();

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

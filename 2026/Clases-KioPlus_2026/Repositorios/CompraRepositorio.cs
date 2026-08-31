using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public class CompraRepositorio : ICompraRepositorio
{
    private readonly ApplicationDbContext _db;
    public CompraRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<bool> ProveedorExiste(int idProveedor) =>
        await _db.Proveedores.AnyAsync(p => p.Id == idProveedor);

    public async Task<IEnumerable<CompraProveedor>> ObtenerTodas() =>
        await _db.Compras.ToListAsync();

    // Listado de compras con el nombre del proveedor y filtros opcionales de fecha
    public async Task<IEnumerable<(CompraProveedor Compra, string Proveedor)>> ObtenerTodasConProveedor(
        DateTime? fechaDesde, DateTime? fechaHasta, int? idProveedor)
    {
        var query = _db.Compras.AsQueryable();

        if (fechaDesde.HasValue)
            query = query.Where(c => c.FechaHora >= fechaDesde.Value.Date);
        if (fechaHasta.HasValue)
            query = query.Where(c => c.FechaHora < fechaHasta.Value.Date.AddDays(1));
        if (idProveedor.HasValue)
            query = query.Where(c => c.ProveedorId == idProveedor.Value);

        var filas = await (
            from c in query
            join p in _db.Proveedores on c.ProveedorId equals p.Id into gp
            from p in gp.DefaultIfEmpty()
            orderby c.FechaHora descending
            select new { Compra = c, Nombre = p != null ? p.NombreRazonSocial : "(proveedor eliminado)" }
        ).ToListAsync();

        return filas.Select(f => (f.Compra, f.Nombre));
    }

    public async Task<CompraProveedor?> ObtenerPorId(int id) =>
        await _db.Compras.FindAsync(id);

    public async Task<CompraProveedor> Agregar(CompraProveedor compra)
    {
        _db.Compras.Add(compra);
        await _db.SaveChangesAsync();
        return compra;
    }

    public async Task Actualizar(CompraProveedor compra)
    {
        _db.Compras.Update(compra);
        await _db.SaveChangesAsync();
    }

    public async Task Eliminar(CompraProveedor compra)
    {
        _db.Compras.Remove(compra);
        await _db.SaveChangesAsync();
    }
}

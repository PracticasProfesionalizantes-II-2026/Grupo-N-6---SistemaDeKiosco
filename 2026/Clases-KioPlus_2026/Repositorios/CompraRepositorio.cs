using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface ICompraRepositorio
{
    Task<bool> ProveedorExiste(int idProveedor);
    Task<IEnumerable<CompraProveedor>> ObtenerTodas();
    Task<CompraProveedor?> ObtenerPorId(int id);
    Task<CompraProveedor> Agregar(CompraProveedor compra);
    Task Actualizar(CompraProveedor compra);
    Task Eliminar(CompraProveedor compra);
}

public class CompraRepositorio : ICompraRepositorio
{
    private readonly ApplicationDbContext _db;
    public CompraRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<bool> ProveedorExiste(int idProveedor) =>
        await _db.Proveedores.AnyAsync(p => p.Id == idProveedor);

    public async Task<IEnumerable<CompraProveedor>> ObtenerTodas() =>
        await _db.Compras.ToListAsync();

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

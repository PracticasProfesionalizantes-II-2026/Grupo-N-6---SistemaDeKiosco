using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public class ProveedorRepositorio : IProveedorRepositorio
{
    private readonly ApplicationDbContext _db;
    public ProveedorRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Proveedor>> ObtenerTodos(string? busqueda)
    {
        var query = _db.Proveedores.AsQueryable();
        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p => p.NombreRazonSocial.Contains(busqueda));
        return await query.ToListAsync();
    }

    public async Task<Proveedor?> ObtenerPorId(int id) =>
        await _db.Proveedores.FindAsync(id);

    public async Task<Proveedor> Agregar(Proveedor proveedor)
    {
        _db.Proveedores.Add(proveedor);
        await _db.SaveChangesAsync();
        return proveedor;
    }

    public async Task Actualizar(Proveedor proveedor)
    {
        _db.Proveedores.Update(proveedor);
        await _db.SaveChangesAsync();
    }

    public async Task Eliminar(Proveedor proveedor)
    {
        _db.Proveedores.Remove(proveedor);
        await _db.SaveChangesAsync();
    }
}

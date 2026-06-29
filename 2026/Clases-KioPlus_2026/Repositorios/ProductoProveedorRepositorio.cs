using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface IProductoProveedorRepositorio
{
    Task<bool> ProductoExiste(int idProducto);
    Task<IEnumerable<ProductoProveedor>> ObtenerPorProducto(int idProducto);
    Task<ProductoProveedor?> ObtenerPorId(int id);
    Task<ProductoProveedor> Agregar(ProductoProveedor pp);
    Task Actualizar(ProductoProveedor pp);
    Task Eliminar(ProductoProveedor pp);
}

public class ProductoProveedorRepositorio : IProductoProveedorRepositorio
{
    private readonly ApplicationDbContext _db;
    public ProductoProveedorRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<bool> ProductoExiste(int idProducto) =>
        await _db.Productos.AnyAsync(p => p.Id == idProducto);

    public async Task<IEnumerable<ProductoProveedor>> ObtenerPorProducto(int idProducto) =>
        await _db.ProductoProveedores.Where(pp => pp.ProductoId == idProducto).ToListAsync();

    public async Task<ProductoProveedor?> ObtenerPorId(int id) =>
        await _db.ProductoProveedores.FindAsync(id);

    public async Task<ProductoProveedor> Agregar(ProductoProveedor pp)
    {
        _db.ProductoProveedores.Add(pp);
        await _db.SaveChangesAsync();
        return pp;
    }

    public async Task Actualizar(ProductoProveedor pp)
    {
        _db.ProductoProveedores.Update(pp);
        await _db.SaveChangesAsync();
    }

    public async Task Eliminar(ProductoProveedor pp)
    {
        _db.ProductoProveedores.Remove(pp);
        await _db.SaveChangesAsync();
    }
}

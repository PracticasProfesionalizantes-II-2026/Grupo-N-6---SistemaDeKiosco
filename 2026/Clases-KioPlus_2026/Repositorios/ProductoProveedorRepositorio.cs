using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public class ProductoProveedorRepositorio : IProductoProveedorRepositorio
{
    private readonly ApplicationDbContext _db;
    public ProductoProveedorRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<bool> ProductoExiste(int idProducto) =>
        await _db.Productos.AnyAsync(p => p.Id == idProducto);

    public async Task<bool> ProveedorExiste(int idProveedor) =>
        await _db.Proveedores.AnyAsync(p => p.Id == idProveedor);

    // Mismo vínculo que ObtenerPorProducto, recorrido al revés
    public async Task<IEnumerable<(ProductoProveedor Asociacion, string Producto, string Marca)>> ObtenerPorProveedor(int idProveedor)
    {
        var filas = await (
            from pp in _db.ProductoProveedores
            join p in _db.Productos on pp.ProductoId equals p.Id into gp
            from p in gp.DefaultIfEmpty()
            where pp.ProveedorId == idProveedor
            orderby p.Nombre
            select new
            {
                Asociacion = pp,
                Nombre = p != null ? p.Nombre : "(producto eliminado)",
                Marca = p != null ? p.Marca : ""
            }
        ).ToListAsync();

        return filas.Select(f => (f.Asociacion, f.Nombre, f.Marca));
    }

    public async Task<IEnumerable<ProductoProveedor>> ObtenerPorProducto(int idProducto) =>
        await _db.ProductoProveedores.Where(pp => pp.ProductoId == idProducto).ToListAsync();

    public async Task<ProductoProveedor?> ObtenerPorProductoYProveedor(int idProducto, int idProveedor) =>
        await _db.ProductoProveedores
            .FirstOrDefaultAsync(pp => pp.ProductoId == idProducto && pp.ProveedorId == idProveedor);

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

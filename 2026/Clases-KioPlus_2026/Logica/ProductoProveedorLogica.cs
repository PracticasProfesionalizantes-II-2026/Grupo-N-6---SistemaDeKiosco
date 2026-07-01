using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;



public class ProductoProveedorLogica : IProductoProveedorLogica
{
    private readonly IProductoProveedorRepositorio _repo;
    public ProductoProveedorLogica(IProductoProveedorRepositorio repo) => _repo = repo;

    private static ProductoProveedorDto AMapa(ProductoProveedor pp) =>
        new(pp.Id, pp.ProveedorId, pp.PrecioCompra);

    public async Task<IEnumerable<ProductoProveedorDto>> ObtenerPorProducto(int idProducto)
    {
        var lista = await _repo.ObtenerPorProducto(idProducto);
        return lista.Select(AMapa);
    }

    public async Task<ProductoProveedorDto?> ObtenerPorId(int id)
    {
        var pp = await _repo.ObtenerPorId(id);
        return pp is null ? null : AMapa(pp);
    }

    // Devuelve null si el producto no existe
    public async Task<int?> Crear(int idProducto, ProductoProveedorCreateDto dto)
    {
        if (!await _repo.ProductoExiste(idProducto)) return null;

        var pp = new ProductoProveedor
        {
            ProductoId = idProducto,
            ProveedorId = dto.IdProveedor,
            PrecioCompra = dto.PrecioCompra
        };
        await _repo.Agregar(pp);
        return pp.Id;
    }

    public async Task<bool> Actualizar(int id, ProductoProveedorUpdateDto dto)
    {
        var pp = await _repo.ObtenerPorId(id);
        if (pp is null) return false;

        pp.PrecioCompra = dto.PrecioCompra;
        await _repo.Actualizar(pp);
        return true;
    }

    public async Task<bool> Eliminar(int id)
    {
        var pp = await _repo.ObtenerPorId(id);
        if (pp is null) return false;

        await _repo.Eliminar(pp);
        return true;
    }
}

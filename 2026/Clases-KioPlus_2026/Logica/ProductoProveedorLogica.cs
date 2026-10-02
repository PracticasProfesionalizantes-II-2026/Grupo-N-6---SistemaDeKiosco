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

    // Devuelve null si el proveedor no existe, para poder responder 404
    public async Task<IEnumerable<ProductoDelProveedorDto>?> ObtenerPorProveedor(int idProveedor)
    {
        if (!await _repo.ProveedorExiste(idProveedor)) return null;

        var filas = await _repo.ObtenerPorProveedor(idProveedor);
        return filas.Select(f => new ProductoDelProveedorDto(
            f.Asociacion.Id, f.Asociacion.ProductoId, f.Producto, f.Marca, f.Asociacion.PrecioCompra));
    }

    public async Task<ProductoProveedorDto?> ObtenerPorId(int id)
    {
        var pp = await _repo.ObtenerPorId(id);
        return pp is null ? null : AMapa(pp);
    }

    // Un producto se asocia una sola vez a cada proveedor: si ya están
    // asociados, lo que corresponde es editar el precio de esa asociación.
    public async Task<ResultadoOperacion> Crear(int idProducto, ProductoProveedorCreateDto dto)
    {
        if (!await _repo.ProductoExiste(idProducto))
            return ResultadoOperacion.NoEncontrado("producto no encontrado");

        if (!await _repo.ProveedorExiste(dto.IdProveedor))
            return ResultadoOperacion.NoEncontrado("proveedor no encontrado");

        if (await _repo.ObtenerPorProductoYProveedor(idProducto, dto.IdProveedor) is not null)
            return ResultadoOperacion.Invalido(
                "ese proveedor ya está asociado a este producto: editá su precio");

        var pp = new ProductoProveedor
        {
            ProductoId = idProducto,
            ProveedorId = dto.IdProveedor,
            PrecioCompra = dto.PrecioCompra
        };
        await _repo.Agregar(pp);
        return ResultadoOperacion.Exito(pp.Id);
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

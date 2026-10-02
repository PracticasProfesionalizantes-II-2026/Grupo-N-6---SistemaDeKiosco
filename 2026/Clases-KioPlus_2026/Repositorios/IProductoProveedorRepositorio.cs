using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface IProductoProveedorRepositorio
{
    Task<bool> ProductoExiste(int idProducto);
    Task<bool> ProveedorExiste(int idProveedor);
    Task<IEnumerable<ProductoProveedor>> ObtenerPorProducto(int idProducto);
    // La misma relación vista desde el proveedor, con el nombre del producto resuelto
    Task<IEnumerable<(ProductoProveedor Asociacion, string Producto, string Marca)>> ObtenerPorProveedor(int idProveedor);
    Task<ProductoProveedor?> ObtenerPorId(int id);
    // La asociación entre un producto y un proveedor, si existe
    Task<ProductoProveedor?> ObtenerPorProductoYProveedor(int idProducto, int idProveedor);
    Task<ProductoProveedor> Agregar(ProductoProveedor pp);
    Task Actualizar(ProductoProveedor pp);
    Task Eliminar(ProductoProveedor pp);
}
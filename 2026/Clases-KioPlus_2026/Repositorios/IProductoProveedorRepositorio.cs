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
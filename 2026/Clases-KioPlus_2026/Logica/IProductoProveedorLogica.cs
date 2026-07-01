using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface IProductoProveedorLogica
{
    Task<IEnumerable<ProductoProveedorDto>> ObtenerPorProducto(int idProducto);
    Task<ProductoProveedorDto?> ObtenerPorId(int id);
    Task<int?> Crear(int idProducto, ProductoProveedorCreateDto dto);
    Task<bool> Actualizar(int id, ProductoProveedorUpdateDto dto);
    Task<bool> Eliminar(int id);
}
using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface IDetalleCompraLogica
{
    Task<IEnumerable<DetalleCompraDto>> ObtenerPorCompra(int idCompra);
    Task<DetalleCompraDto?> ObtenerPorId(int id);
    Task<int?> Crear(int idCompra, DetalleCompraCreateDto dto);
    Task<bool> Actualizar(int id, DetalleCompraUpdateDto dto);
    Task<bool> Eliminar(int id);
}
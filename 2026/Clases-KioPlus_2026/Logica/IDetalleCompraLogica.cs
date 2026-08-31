using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface IDetalleCompraLogica
{
    Task<IEnumerable<DetalleCompraListadoDto>> ObtenerPorCompra(int idCompra);
    Task<DetalleCompraDto?> ObtenerPorId(int id);
    Task<ResultadoOperacion> Crear(int idCompra, DetalleCompraCreateDto dto);
    Task<ResultadoOperacion> Actualizar(int id, DetalleCompraUpdateDto dto);
    Task<bool> Eliminar(int id);
}

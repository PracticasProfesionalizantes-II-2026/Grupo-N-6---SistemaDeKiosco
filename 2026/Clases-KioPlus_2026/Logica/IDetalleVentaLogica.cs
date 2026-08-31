using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface IDetalleVentaLogica
{
    Task<IEnumerable<DetalleVentaListadoDto>> ObtenerPorVenta(int idVenta);
    Task<DetalleVentaDto?> ObtenerPorId(int id);
    Task<ResultadoOperacion> Crear(int idVenta, DetalleVentaCreateDto dto);
    Task<ResultadoOperacion> Actualizar(int idVenta, int id, DetalleVentaUpdateDto dto);
    Task<bool> Eliminar(int idVenta, int id);
}

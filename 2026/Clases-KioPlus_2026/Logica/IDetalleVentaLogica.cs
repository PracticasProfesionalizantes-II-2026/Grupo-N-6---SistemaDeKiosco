using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface IDetalleVentaLogica
{
    Task<IEnumerable<DetalleVentaDto>> ObtenerPorVenta(int idVenta);
    Task<DetalleVentaDto?> ObtenerPorId(int id);
    Task<int?> Crear(int idVenta, DetalleVentaCreateDto dto);
    Task<bool> Actualizar(int idVenta, int id, DetalleVentaUpdateDto dto);
    Task<bool> Eliminar(int idVenta, int id);
}
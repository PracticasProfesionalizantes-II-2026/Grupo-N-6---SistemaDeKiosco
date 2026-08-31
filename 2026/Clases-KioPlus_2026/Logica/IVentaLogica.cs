using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface IVentaLogica
{
    Task<IEnumerable<VentaListadoDto>> ObtenerTodas(
        DateTime? fechaDesde, DateTime? fechaHasta, int? idUsuario, int? idCliente,
        double? importeMayorA, double? importeMenorA);
    Task<VentaDto?> ObtenerPorId(int id);
    Task<ResultadoOperacion> Crear(VentaCreateDto dto);
    Task<bool> Actualizar(int id, VentaCreateDto dto);
    Task<ResultadoOperacion> Finalizar(int id);
    Task<bool> Eliminar(int id);
}

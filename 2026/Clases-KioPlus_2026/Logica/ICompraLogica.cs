using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface ICompraLogica
{
    Task<IEnumerable<CompraListadoDto>> ObtenerTodas(DateTime? fechaDesde, DateTime? fechaHasta, int? idProveedor);
    Task<CompraDto?> ObtenerPorId(int id);
    Task<int?> Crear(CompraCreateDto dto);
    Task<bool> Actualizar(int id, CompraCreateDto dto);
    Task<bool> Eliminar(int id);
}

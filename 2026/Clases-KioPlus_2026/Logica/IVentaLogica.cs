using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface IVentaLogica
{
    Task<IEnumerable<VentaDto>> ObtenerTodas();
    Task<IEnumerable<VentaFiltroDto>> Filtrar(
        DateTime? fechaHora, int? idUsuario, int? idCliente,
        double? importeMayorA, double? importeMenorA);
    Task<VentaDto?> ObtenerPorId(int id);
    Task<int?> Crear(VentaCreateDto dto);
    Task<bool> Actualizar(int id, VentaCreateDto dto);
    Task<bool> Eliminar(int id);
}
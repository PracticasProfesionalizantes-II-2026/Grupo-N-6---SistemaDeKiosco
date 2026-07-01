using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface ILoteLogica
{
    Task<IEnumerable<LoteDto>> ObtenerPorProducto(int idProducto);
    Task<LoteDto?> ObtenerPorId(int id);
    Task<int?> Crear(int idProducto, LoteCreateDto dto);
    Task<bool> Actualizar(int id, LoteCreateDto dto);
    Task<bool> Eliminar(int id);
}
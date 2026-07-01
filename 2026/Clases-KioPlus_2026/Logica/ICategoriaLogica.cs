using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface ICategoriaLogica
{
    Task<IEnumerable<CategoriaDto>> ObtenerTodas();
    Task<CategoriaDto?> ObtenerPorId(int id);
    Task<int> Crear(CategoriaCreateDto dto);
    Task<bool> Actualizar(int id, CategoriaCreateDto dto);
    Task<bool> Eliminar(int id);
}
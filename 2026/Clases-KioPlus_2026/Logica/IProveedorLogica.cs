using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;

public interface IProveedorLogica
{
    Task<IEnumerable<ProveedorDto>> ObtenerTodos(string? busqueda);
    Task<ProveedorDto?> ObtenerPorId(int id);
    Task<int> Crear(ProveedorCreateDto dto);
    Task<bool> Actualizar(int id, ProveedorCreateDto dto);
    Task<bool> Eliminar(int id);
}
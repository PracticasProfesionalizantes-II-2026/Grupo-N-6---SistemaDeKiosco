using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface IUsuarioLogica
{
    Task<IEnumerable<UsuarioDto>> ObtenerTodos();
    Task<UsuarioDto?> ObtenerPorId(int id);
    Task<int> Crear(UsuarioCreateDto dto);
    Task<bool> Actualizar(int id, UsuarioCreateDto dto);
    Task<bool> Eliminar(int id);
}
using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface IUsuarioLogica
{
    Task<IEnumerable<UsuarioDto>> ObtenerTodos();
    Task<UsuarioDto?> ObtenerPorId(int id);
    Task<ResultadoOperacion> Crear(UsuarioCreateDto dto);
    Task<ResultadoOperacion> Actualizar(int id, UsuarioCreateDto dto);
    Task<bool> Eliminar(int id);
    Task<bool> CambiarEstado(int id, bool estado);
    Task<LoginResultadoDto?> Login(LoginDto dto);
}

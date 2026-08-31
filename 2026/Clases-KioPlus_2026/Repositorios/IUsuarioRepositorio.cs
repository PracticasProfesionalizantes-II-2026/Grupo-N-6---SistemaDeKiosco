using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface IUsuarioRepositorio
{
    Task<IEnumerable<Usuario>> ObtenerTodos();
    Task<Usuario?> ObtenerPorId(int id);
    Task<Usuario?> ObtenerPorNombreUsuario(string nombreUsuario);
    Task<bool> NombreUsuarioEnUso(string nombreUsuario, int? idExcluido);
    Task<Usuario> Agregar(Usuario usuario);
    Task Actualizar(Usuario usuario);
    Task Eliminar(Usuario usuario);
}

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

public class UsuarioLogica : IUsuarioLogica
{
    private readonly IUsuarioRepositorio _repo;
    public UsuarioLogica(IUsuarioRepositorio repo) => _repo = repo;

    private static UsuarioDto AMapa(Usuario u) =>
        new(u.Id, u.NombreApellido, u.Telefono, u.NombreUsuario, u.ContraseniaUsuario, u.TipoUsuario, u.Estado);

    public async Task<IEnumerable<UsuarioDto>> ObtenerTodos()
    {
        var usuarios = await _repo.ObtenerTodos();
        return usuarios.Select(AMapa);
    }

    public async Task<UsuarioDto?> ObtenerPorId(int id)
    {
        var u = await _repo.ObtenerPorId(id);
        return u is null ? null : AMapa(u);
    }

    public async Task<int> Crear(UsuarioCreateDto dto)
    {
        var usuario = new Usuario
        {
            NombreApellido = dto.NombreApellido,
            Telefono = dto.Telefono,
            NombreUsuario = dto.NombreUsuario,
            ContraseniaUsuario = dto.ContraseniaUsuario,
            TipoUsuario = dto.TipoUsuario,
            Estado = dto.Estado
        };
        await _repo.Agregar(usuario);
        return usuario.Id;
    }

    public async Task<bool> Actualizar(int id, UsuarioCreateDto dto)
    {
        var usuario = await _repo.ObtenerPorId(id);
        if (usuario is null) return false;

        usuario.NombreApellido = dto.NombreApellido;
        usuario.Telefono = dto.Telefono;
        usuario.NombreUsuario = dto.NombreUsuario;
        usuario.ContraseniaUsuario = dto.ContraseniaUsuario;
        usuario.TipoUsuario = dto.TipoUsuario;
        usuario.Estado = dto.Estado;
        await _repo.Actualizar(usuario);
        return true;
    }

    public async Task<bool> Eliminar(int id)
    {
        var usuario = await _repo.ObtenerPorId(id);
        if (usuario is null) return false;

        await _repo.Eliminar(usuario);
        return true;
    }
}

using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


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

    public async Task<ResultadoOperacion> Crear(UsuarioCreateDto dto)
    {
        if (await _repo.NombreUsuarioEnUso(dto.NombreUsuario, null))
            return ResultadoOperacion.Invalido("el nombre de usuario ya está en uso");

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
        return ResultadoOperacion.Exito(usuario.Id);
    }

    public async Task<ResultadoOperacion> Actualizar(int id, UsuarioCreateDto dto)
    {
        var usuario = await _repo.ObtenerPorId(id);
        if (usuario is null) return ResultadoOperacion.NoEncontrado("usuario no encontrado");

        if (await _repo.NombreUsuarioEnUso(dto.NombreUsuario, id))
            return ResultadoOperacion.Invalido("el nombre de usuario ya está en uso");

        usuario.NombreApellido = dto.NombreApellido;
        usuario.Telefono = dto.Telefono;
        usuario.NombreUsuario = dto.NombreUsuario;
        usuario.ContraseniaUsuario = dto.ContraseniaUsuario;
        usuario.TipoUsuario = dto.TipoUsuario;
        usuario.Estado = dto.Estado;
        await _repo.Actualizar(usuario);
        return ResultadoOperacion.Exito(usuario.Id);
    }

    public async Task<bool> Eliminar(int id)
    {
        var usuario = await _repo.ObtenerPorId(id);
        if (usuario is null) return false;

        await _repo.Eliminar(usuario);
        return true;
    }

    // Habilita o bloquea el acceso del usuario sin borrar su historial de ventas
    public async Task<bool> CambiarEstado(int id, bool estado)
    {
        var usuario = await _repo.ObtenerPorId(id);
        if (usuario is null) return false;

        usuario.Estado = estado;
        await _repo.Actualizar(usuario);
        return true;
    }

    // Devuelve null si las credenciales no coinciden o el usuario está bloqueado
    public async Task<LoginResultadoDto?> Login(LoginDto dto)
    {
        var usuario = await _repo.ObtenerPorNombreUsuario(dto.NombreUsuario);
        if (usuario is null) return null;
        if (!usuario.Estado) return null;
        if (usuario.ContraseniaUsuario != dto.ContraseniaUsuario) return null;

        return new LoginResultadoDto(
            usuario.Id, usuario.NombreApellido, usuario.NombreUsuario, usuario.TipoUsuario);
    }
}

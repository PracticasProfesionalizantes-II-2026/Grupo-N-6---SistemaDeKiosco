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

    // Restablece la contraseña desde "¿Olvidaste tu contraseña?".
    // Un usuario bloqueado no puede recuperarla por su cuenta.
    public async Task<ResultadoOperacion> CambiarContrasenia(CambiarContraseniaDto dto)
    {
        var usuario = await _repo.ObtenerPorNombreUsuario(dto.NombreUsuario);
        if (usuario is null) return ResultadoOperacion.NoEncontrado("usuario no encontrado");

        if (!usuario.Estado)
            return ResultadoOperacion.Invalido("este usuario se encuentra bloqueado");

        usuario.ContraseniaUsuario = dto.NuevaContrasenia;
        await _repo.Actualizar(usuario);
        return ResultadoOperacion.Exito(usuario.Id);
    }

    // Sin sesión si las credenciales no coinciden. Un usuario bloqueado se informa
    // como tal, igual que en "¿Olvidaste tu contraseña?".
    public async Task<ResultadoLoginDto> Login(LoginDto dto)
    {
        var usuario = await _repo.ObtenerPorNombreUsuario(dto.NombreUsuario);
        if (usuario is null) return new ResultadoLoginDto(null);
        if (!usuario.Estado) return new ResultadoLoginDto(null, Bloqueado: true);
        if (usuario.ContraseniaUsuario != dto.ContraseniaUsuario) return new ResultadoLoginDto(null);

        return new ResultadoLoginDto(new LoginResultadoDto(
            usuario.Id, usuario.NombreApellido, usuario.NombreUsuario, usuario.TipoUsuario));
    }
}

using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class UsuarioLogica : IUsuarioLogica
{
    private readonly IUsuarioRepositorio _repo;
    private readonly ControlIntentosLogin _intentos;

    public UsuarioLogica(IUsuarioRepositorio repo, ControlIntentosLogin intentos)
    {
        _repo = repo;
        _intentos = intentos;
    }

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
            ContraseniaUsuario = HashContrasenia.Calcular(dto.ContraseniaUsuario),
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
        // Al editar sin cambiar la contraseña, el front reenvía el hash que ya estaba
        // guardado: en ese caso se conserva para no hashear dos veces.
        if (dto.ContraseniaUsuario != usuario.ContraseniaUsuario)
            usuario.ContraseniaUsuario = HashContrasenia.Calcular(dto.ContraseniaUsuario);
        usuario.TipoUsuario = dto.TipoUsuario;
        usuario.Estado = dto.Estado;
        await _repo.Actualizar(usuario);
        return ResultadoOperacion.Exito(usuario.Id);
    }

    public async Task<bool> Eliminar(int id)
    {
        var usuario = await _repo.ObtenerPorId(id);
        if (usuario is null || !usuario.Activo) return false;

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

        usuario.ContraseniaUsuario = HashContrasenia.Calcular(dto.NuevaContrasenia);
        await _repo.Actualizar(usuario);
        return ResultadoOperacion.Exito(usuario.Id);
    }

    // Sin sesión si las credenciales no coinciden. Un usuario bloqueado se informa
    // como tal, igual que en "¿Olvidaste tu contraseña?", y no cuenta como intento
    // fallido. Tras 5 fallos seguidos hay que esperar 1 minuto antes de reintentar.
    public async Task<ResultadoLoginDto> Login(LoginDto dto)
    {
        if (_intentos.EstaEsperando(dto.NombreUsuario))
            return new ResultadoLoginDto(null, DemasiadosIntentos: true);

        var usuario = await _repo.ObtenerPorNombreUsuario(dto.NombreUsuario);
        if (usuario is null)
        {
            _intentos.RegistrarFallo(dto.NombreUsuario);
            return new ResultadoLoginDto(null);
        }
        if (!usuario.Estado) return new ResultadoLoginDto(null, Bloqueado: true);
        if (usuario.ContraseniaUsuario != HashContrasenia.Calcular(dto.ContraseniaUsuario))
        {
            _intentos.RegistrarFallo(dto.NombreUsuario);
            return new ResultadoLoginDto(null);
        }

        _intentos.Reiniciar(dto.NombreUsuario);
        return new ResultadoLoginDto(new LoginResultadoDto(
            usuario.Id, usuario.NombreApellido, usuario.NombreUsuario, usuario.TipoUsuario));
    }
}

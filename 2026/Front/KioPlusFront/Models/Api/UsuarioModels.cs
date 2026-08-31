namespace KioPlusFront.Models.Api;

// Espejo de los enums y DTOs que expone la API. Los enums viajan como texto.
public enum TipoDeUsuario
{
    SuperAdmin,
    Administrador,
    Empleado
}

public record UsuarioDto(
    int IdUsuario,
    string NombreApellido,
    string Telefono,
    string NombreUsuario,
    string ContraseniaUsuario,
    TipoDeUsuario TipoUsuario,
    bool Estado);

public record UsuarioCreateDto(
    string NombreApellido,
    string Telefono,
    string NombreUsuario,
    string ContraseniaUsuario,
    TipoDeUsuario TipoUsuario,
    bool Estado);

public record LoginDto(string NombreUsuario, string ContraseniaUsuario);

public record LoginResultadoDto(
    int IdUsuario,
    string NombreApellido,
    string NombreUsuario,
    TipoDeUsuario TipoUsuario);

public record CambiarEstadoUsuarioDto(bool Estado);

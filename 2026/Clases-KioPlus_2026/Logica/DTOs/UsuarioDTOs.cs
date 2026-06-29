using Clases_KioPlus.Models;

namespace Clases_KioPlus.Logica.DTOs;

public record UsuarioDto(
    int IdUsuario,
    string NombreApellido,
    string Telefono,
    string NombreUsuario,
    string ContraseniaUsuario,
    Usuario.TipoDeUsuario TipoUsuario,
    bool Estado);

public record UsuarioCreateDto(
    string NombreApellido,
    string Telefono,
    string NombreUsuario,
    string ContraseniaUsuario,
    Usuario.TipoDeUsuario TipoUsuario,
    bool Estado);

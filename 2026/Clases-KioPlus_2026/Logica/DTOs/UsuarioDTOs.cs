using System.ComponentModel.DataAnnotations;
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
    [property: Required] string NombreApellido,
    [property: Required] string Telefono,
    [property: Required] string NombreUsuario,
    [property: Required] string ContraseniaUsuario,
    Usuario.TipoDeUsuario TipoUsuario,
    bool Estado);

// Credenciales enviadas por la pantalla de inicio de sesión
public record LoginDto(
    [property: Required] string NombreUsuario,
    [property: Required] string ContraseniaUsuario);

// Datos de la sesión iniciada. Nunca incluye la contraseña.
public record LoginResultadoDto(
    int IdUsuario,
    string NombreApellido,
    string NombreUsuario,
    Usuario.TipoDeUsuario TipoUsuario);

// Alta/baja lógica del usuario (candado en el listado)
public record CambiarEstadoUsuarioDto(bool Estado);

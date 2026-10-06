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
    [property: Required(ErrorMessage = "Ingresá el nombre y apellido")]
    [property: RegularExpression(Formatos.SoloLetras, ErrorMessage = "El nombre y apellido " + Formatos.MensajeSoloLetras)]
    string NombreApellido,
    [property: Required(ErrorMessage = "Ingresá el teléfono")]
    [property: RegularExpression(Formatos.Telefono, ErrorMessage = Formatos.MensajeTelefono)]
    string Telefono,
    [property: Required(ErrorMessage = "Ingresá el nombre de usuario")] string NombreUsuario,
    [property: Required(ErrorMessage = "Ingresá la contraseña")] string ContraseniaUsuario,
    Usuario.TipoDeUsuario TipoUsuario,
    bool Estado);

// Credenciales enviadas por la pantalla de inicio de sesión
public record LoginDto(
    [property: Required(ErrorMessage = "Ingresá el nombre de usuario")] string NombreUsuario,
    [property: Required(ErrorMessage = "Ingresá la contraseña")] string ContraseniaUsuario);

// Datos de la sesión iniciada. Nunca incluye la contraseña.
public record LoginResultadoDto(
    int IdUsuario,
    string NombreApellido,
    string NombreUsuario,
    Usuario.TipoDeUsuario TipoUsuario);

// Resultado del intento de inicio de sesión. Sesion queda en null cuando las
// credenciales no coinciden o el usuario está bloqueado (Bloqueado = true).
public record ResultadoLoginDto(LoginResultadoDto? Sesion, bool Bloqueado = false);

// Alta/baja lógica del usuario (candado en el listado)
public record CambiarEstadoUsuarioDto(bool Estado);

// Nueva contraseña pedida desde "¿Olvidaste tu contraseña?".
// La confirmación se valida en la pantalla antes de llegar acá.
public record CambiarContraseniaDto(
    [property: Required(ErrorMessage = "Ingresá el nombre de usuario")] string NombreUsuario,
    [property: Required(ErrorMessage = "Ingresá la contraseña nueva")] string NuevaContrasenia);

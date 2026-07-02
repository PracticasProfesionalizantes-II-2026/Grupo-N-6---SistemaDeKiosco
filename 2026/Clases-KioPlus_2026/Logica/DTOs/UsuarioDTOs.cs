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

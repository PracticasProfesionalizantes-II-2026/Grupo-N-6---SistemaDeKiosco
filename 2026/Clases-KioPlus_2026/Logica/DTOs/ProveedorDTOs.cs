using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record ProveedorDto(
    int IdProveedor,
    string NombreRazonSocial,
    string Telefono,
    string Direccion,
    string? CorreoElectronico,
    string? Observaciones);

public record ProveedorCreateDto(
    [property: Required(ErrorMessage = "Ingresá el nombre o razón social")] string NombreRazonSocial,
    [property: Required(ErrorMessage = "Ingresá el teléfono")] string Telefono,
    [property: Required(ErrorMessage = "Ingresá la dirección")] string Direccion,
    [property: EmailAddress(ErrorMessage = "El correo electrónico no es válido")] string? CorreoElectronico,
    string? Observaciones);

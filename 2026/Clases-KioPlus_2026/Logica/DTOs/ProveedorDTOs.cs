using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record ProveedorDto(
    int IdProveedor,
    string NombreRazonSocial,
    string Telefono,
    string Direccion,
    string CorreoElectronico,
    string Observaciones);

public record ProveedorCreateDto(
    [property: Required] string NombreRazonSocial,
    [property: Required] string Telefono,
    [property: Required] string Direccion,
    [property: Required, EmailAddress] string CorreoElectronico,
    string Observaciones);

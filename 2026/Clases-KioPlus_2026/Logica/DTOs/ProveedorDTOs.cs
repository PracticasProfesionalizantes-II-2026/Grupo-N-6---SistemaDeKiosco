namespace Clases_KioPlus.Logica.DTOs;

public record ProveedorDto(
    int IdProveedor,
    string NombreRazonSocial,
    string Telefono,
    string Direccion,
    string CorreoElectronico,
    string Observaciones);

public record ProveedorCreateDto(
    string NombreRazonSocial,
    string Telefono,
    string Direccion,
    string CorreoElectronico,
    string Observaciones);

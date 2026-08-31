namespace KioPlusFront.Models.Api;

public enum EstadoDeuda
{
    Moroso,
    AlDia
}

public record CuentaCorrienteClienteDto(
    int IdCuentaCorrienteCliente,
    string Nombre,
    string Apellido,
    int Dni,
    string Telefono,
    string Direccion,
    string CorreoElectronico,
    double MontoAdeudado,
    EstadoDeuda Estado);

public record CuentaCorrienteClienteCreateDto(
    string Nombre,
    string Apellido,
    int Dni,
    string Telefono,
    string Direccion,
    string? CorreoElectronico,
    double MontoAdeudado);

public record PagoCuentaCorrienteDto(double Monto);

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
    string? CorreoElectronico,
    double MontoAdeudado,
    EstadoDeuda Estado,
    DateTime FechaAlta);

public record CuentaCorrienteClienteCreateDto(
    string Nombre,
    string Apellido,
    int Dni,
    string Telefono,
    string Direccion,
    string? CorreoElectronico,
    double MontoAdeudado);

public record PagoCuentaCorrienteDto(double Monto);

// Cancelación de toda la deuda del cliente. Sin fecha se toma la de hoy.
public record PagoDeudaTotalDto(DateTime? FechaPago);

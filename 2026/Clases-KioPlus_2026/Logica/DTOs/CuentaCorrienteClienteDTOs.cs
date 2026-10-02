using System.ComponentModel.DataAnnotations;
using Clases_KioPlus.Models;

namespace Clases_KioPlus.Logica.DTOs;

public record CuentaCorrienteClienteDto(
    int IdCuentaCorrienteCliente,
    string Nombre,
    string Apellido,
    int Dni,
    string Telefono,
    string Direccion,
    string? CorreoElectronico,
    double MontoAdeudado,
    CuentaCorrienteCliente.EstadoDeuda Estado);

public record CuentaCorrienteClienteCreateDto(
    [property: Required(ErrorMessage = "Ingresá el nombre")] string Nombre,
    [property: Required(ErrorMessage = "Ingresá el apellido")] string Apellido,
    [property: Range(1, int.MaxValue, ErrorMessage = "El DNI tiene que ser un número positivo")] int Dni,
    [property: Required(ErrorMessage = "Ingresá el teléfono")] string Telefono,
    [property: Required(ErrorMessage = "Ingresá la dirección")] string Direccion,
    // El correo es opcional en el formulario; si viene, debe ser válido.
    [property: EmailAddress(ErrorMessage = "El correo electrónico no es válido")] string? CorreoElectronico,
    [property: Range(0, double.MaxValue, ErrorMessage = "El monto adeudado no puede ser negativo")] double MontoAdeudado);

// Pago a cuenta por un monto libre
public record PagoCuentaCorrienteDto(
    [property: Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")] double Monto);

// Cancelación de toda la deuda del cliente. Sin fecha se toma la de hoy.
public record PagoDeudaTotalDto(DateTime? FechaPago);

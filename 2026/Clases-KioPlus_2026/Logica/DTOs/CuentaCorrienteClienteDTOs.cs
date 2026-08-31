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
    string CorreoElectronico,
    double MontoAdeudado,
    CuentaCorrienteCliente.EstadoDeuda Estado);

public record CuentaCorrienteClienteCreateDto(
    [property: Required] string Nombre,
    [property: Required] string Apellido,
    [property: Range(1, int.MaxValue, ErrorMessage = "dni inválido")] int Dni,
    [property: Required] string Telefono,
    [property: Required] string Direccion,
    // El correo es opcional en el formulario; si viene, debe ser válido.
    [property: EmailAddress(ErrorMessage = "correoElectronico inválido")] string? CorreoElectronico,
    [property: Range(0, double.MaxValue, ErrorMessage = "montoAdeudado no puede ser negativo")] double MontoAdeudado);

// Pago parcial o total de la deuda del cliente
public record PagoCuentaCorrienteDto(
    [property: Range(0.01, double.MaxValue, ErrorMessage = "monto debe ser mayor a 0")] double Monto);

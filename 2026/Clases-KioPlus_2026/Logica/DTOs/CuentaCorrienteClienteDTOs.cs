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
    [property: Required, EmailAddress] string CorreoElectronico);

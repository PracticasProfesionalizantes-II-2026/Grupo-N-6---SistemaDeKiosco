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
    string Nombre,
    string Apellido,
    int Dni,
    string Telefono,
    string Direccion,
    string CorreoElectronico);

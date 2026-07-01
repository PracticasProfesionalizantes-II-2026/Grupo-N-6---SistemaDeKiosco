using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface ICuentaCorrienteClienteLogica
{
    Task<IEnumerable<CuentaCorrienteClienteDto>> ObtenerTodas(
        string? nombre, string? apellido, int? dni,
        CuentaCorrienteCliente.EstadoDeuda? estado,
        double? montoAdeudadoMin, double? montoAdeudadoMax);
    Task<CuentaCorrienteClienteDto?> ObtenerPorId(int id);
    Task<int> Crear(CuentaCorrienteClienteCreateDto dto);
    Task<bool> Actualizar(int id, CuentaCorrienteClienteCreateDto dto);
    Task<bool> Eliminar(int id);
}
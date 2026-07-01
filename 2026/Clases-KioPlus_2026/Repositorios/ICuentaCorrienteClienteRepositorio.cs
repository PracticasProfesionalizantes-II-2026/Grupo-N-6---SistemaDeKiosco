using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface ICuentaCorrienteClienteRepositorio
{
    Task<IEnumerable<CuentaCorrienteCliente>> ObtenerTodas(
        string? nombre, string? apellido, int? dni,
        CuentaCorrienteCliente.EstadoDeuda? estado,
        double? montoAdeudadoMin, double? montoAdeudadoMax);
    Task<CuentaCorrienteCliente?> ObtenerPorId(int id);
    Task<CuentaCorrienteCliente> Agregar(CuentaCorrienteCliente cuenta);
    Task Actualizar(CuentaCorrienteCliente cuenta);
    Task Eliminar(CuentaCorrienteCliente cuenta);
}
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
    // Deja MontoAdeudado igual al saldo inicial más lo pendiente de las ventas
    // en cuenta corriente del cliente, y el estado acorde a ese monto.
    Task RecalcularDeuda(int id);

    // Baja el saldo cargado a mano al dar de alta la cuenta, que es lo único que
    // un pago no puede imputar contra una venta
    Task DescontarSaldoInicial(int id, double monto);
}

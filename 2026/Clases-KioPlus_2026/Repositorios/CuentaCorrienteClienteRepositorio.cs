using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;



public class CuentaCorrienteClienteRepositorio : ICuentaCorrienteClienteRepositorio
{
    private readonly ApplicationDbContext _db;
    public CuentaCorrienteClienteRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<CuentaCorrienteCliente>> ObtenerTodas(
        string? nombre, string? apellido, int? dni,
        CuentaCorrienteCliente.EstadoDeuda? estado,
        double? montoAdeudadoMin, double? montoAdeudadoMax)
    {
        var query = _db.CuentasCorrientesClientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(c => c.Nombre.Contains(nombre));
        if (!string.IsNullOrWhiteSpace(apellido))
            query = query.Where(c => c.Apellido.Contains(apellido));
        if (dni.HasValue)
            query = query.Where(c => c.Dni == dni.Value);
        if (estado.HasValue)
            query = query.Where(c => c.Estado == estado.Value);
        if (montoAdeudadoMin.HasValue)
            query = query.Where(c => c.MontoAdeudado >= montoAdeudadoMin.Value);
        if (montoAdeudadoMax.HasValue)
            query = query.Where(c => c.MontoAdeudado <= montoAdeudadoMax.Value);

        return await query.ToListAsync();
    }

    public async Task<CuentaCorrienteCliente?> ObtenerPorId(int id) =>
        await _db.CuentasCorrientesClientes.FindAsync(id);

    public async Task<CuentaCorrienteCliente> Agregar(CuentaCorrienteCliente cuenta)
    {
        _db.CuentasCorrientesClientes.Add(cuenta);
        await _db.SaveChangesAsync();
        return cuenta;
    }

    public async Task Actualizar(CuentaCorrienteCliente cuenta)
    {
        _db.CuentasCorrientesClientes.Update(cuenta);
        await _db.SaveChangesAsync();
    }

    public async Task Eliminar(CuentaCorrienteCliente cuenta)
    {
        _db.CuentasCorrientesClientes.Remove(cuenta);
        await _db.SaveChangesAsync();
    }

    // El monto adeudado nunca se mueve a mano: se reconstruye desde las ventas.
    // Así no puede quedar desincronizado con lo que el cliente ve que debe.
    public async Task RecalcularDeuda(int id)
    {
        var cuenta = await _db.CuentasCorrientesClientes.FindAsync(id);
        if (cuenta is null) return;

        var pendienteDeVentas = await _db.Ventas
            .Where(v => v.CuentaCorrienteClienteId == id
                        && v.Finalizada
                        && v.FormaPago == Venta.FormaDePago.CuentaCorriente
                        && v.Estado == Venta.EstadoVenta.NoPagado)
            .SumAsync(v => (double?)(v.MontoTotal - v.MontoPagado)) ?? 0;

        cuenta.MontoAdeudado = Math.Max(0, cuenta.SaldoInicial) + Math.Max(0, pendienteDeVentas);
        cuenta.Estado = cuenta.MontoAdeudado > 0
            ? CuentaCorrienteCliente.EstadoDeuda.Moroso
            : CuentaCorrienteCliente.EstadoDeuda.AlDia;
        await _db.SaveChangesAsync();
    }

    public async Task DescontarSaldoInicial(int id, double monto)
    {
        var cuenta = await _db.CuentasCorrientesClientes.FindAsync(id);
        if (cuenta is null || monto <= 0) return;

        cuenta.SaldoInicial = Math.Max(0, cuenta.SaldoInicial - monto);
        await _db.SaveChangesAsync();
    }
}

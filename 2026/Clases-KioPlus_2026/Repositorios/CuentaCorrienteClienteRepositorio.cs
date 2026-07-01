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
}

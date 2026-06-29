using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface IVentaRepositorio
{
    Task<bool> UsuarioExiste(int idUsuario);
    Task<bool> CuentaExiste(int idCuenta);
    Task<IEnumerable<Venta>> ObtenerTodas();
    Task<IEnumerable<Venta>> Filtrar(
        DateTime? fechaHora, int? idUsuario, int? idCliente,
        double? importeMayorA, double? importeMenorA);
    Task<Venta?> ObtenerPorId(int id);
    Task<Venta> Agregar(Venta venta);
    Task Actualizar(Venta venta);
    Task Eliminar(Venta venta);
}

public class VentaRepositorio : IVentaRepositorio
{
    private readonly ApplicationDbContext _db;
    public VentaRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<bool> UsuarioExiste(int idUsuario) =>
        await _db.Usuarios.AnyAsync(u => u.Id == idUsuario);

    public async Task<bool> CuentaExiste(int idCuenta) =>
        await _db.CuentasCorrientesClientes.AnyAsync(c => c.Id == idCuenta);

    public async Task<IEnumerable<Venta>> ObtenerTodas() =>
        await _db.Ventas.ToListAsync();

    public async Task<IEnumerable<Venta>> Filtrar(
        DateTime? fechaHora, int? idUsuario, int? idCliente,
        double? importeMayorA, double? importeMenorA)
    {
        var query = _db.Ventas.AsQueryable();

        if (fechaHora.HasValue)
            query = query.Where(v => v.FechaHora.Date == fechaHora.Value.Date);
        if (idUsuario.HasValue)
            query = query.Where(v => v.UsuarioId == idUsuario.Value);
        if (idCliente.HasValue)
            query = query.Where(v => v.CuentaCorrienteClienteId == idCliente.Value);
        if (importeMayorA.HasValue)
            query = query.Where(v => v.MontoTotal > importeMayorA.Value);
        if (importeMenorA.HasValue)
            query = query.Where(v => v.MontoTotal < importeMenorA.Value);

        return await query.ToListAsync();
    }

    public async Task<Venta?> ObtenerPorId(int id) =>
        await _db.Ventas.FindAsync(id);

    public async Task<Venta> Agregar(Venta venta)
    {
        _db.Ventas.Add(venta);
        await _db.SaveChangesAsync();
        return venta;
    }

    public async Task Actualizar(Venta venta)
    {
        _db.Ventas.Update(venta);
        await _db.SaveChangesAsync();
    }

    public async Task Eliminar(Venta venta)
    {
        _db.Ventas.Remove(venta);
        await _db.SaveChangesAsync();
    }
}

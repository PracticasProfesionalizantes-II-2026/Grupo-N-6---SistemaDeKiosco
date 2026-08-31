using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

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

    public async Task<IEnumerable<(Venta Venta, string Vendedor, string Cliente)>> ObtenerTodasConNombres(
        DateTime? fechaDesde, DateTime? fechaHasta, int? idUsuario, int? idCliente,
        double? importeMayorA, double? importeMenorA)
    {
        var query = _db.Ventas.AsQueryable();

        if (fechaDesde.HasValue)
            query = query.Where(v => v.FechaHora >= fechaDesde.Value.Date);
        if (fechaHasta.HasValue)
            query = query.Where(v => v.FechaHora < fechaHasta.Value.Date.AddDays(1));
        if (idUsuario.HasValue)
            query = query.Where(v => v.UsuarioId == idUsuario.Value);
        if (idCliente.HasValue)
            query = query.Where(v => v.CuentaCorrienteClienteId == idCliente.Value);
        if (importeMayorA.HasValue)
            query = query.Where(v => v.MontoTotal > importeMayorA.Value);
        if (importeMenorA.HasValue)
            query = query.Where(v => v.MontoTotal < importeMenorA.Value);

        var filas = await (
            from v in query
            join u in _db.Usuarios on v.UsuarioId equals u.Id into gu
            from u in gu.DefaultIfEmpty()
            join c in _db.CuentasCorrientesClientes on v.CuentaCorrienteClienteId equals c.Id into gc
            from c in gc.DefaultIfEmpty()
            orderby v.FechaHora descending
            select new
            {
                Venta = v,
                Vendedor = u != null ? u.NombreApellido : "(usuario eliminado)",
                Cliente = c != null ? c.Nombre + " " + c.Apellido : "(cliente eliminado)"
            }
        ).ToListAsync();

        return filas.Select(f => (f.Venta, f.Vendedor, f.Cliente.Trim()));
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

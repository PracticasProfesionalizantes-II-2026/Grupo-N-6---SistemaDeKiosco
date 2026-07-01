using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public class CajaRepositorio : ICajaRepositorio
{
    private readonly ApplicationDbContext _db;
    public CajaRepositorio(ApplicationDbContext db) => _db = db;

    // Saldo = ingresos por ventas - egresos por compras
    public async Task<double> SaldoActual()
    {
        var ventas = await _db.Ventas.SumAsync(v => (double?)v.MontoTotal) ?? 0;
        var compras = await _db.Compras.SumAsync(c => (double?)c.MontoTotal) ?? 0;
        return ventas - compras;
    }

    public async Task<double> SaldoAFecha(DateTime fecha)
    {
        var limite = fecha.Date.AddDays(1); // hasta el final del día indicado
        var ventas = await _db.Ventas
            .Where(v => v.FechaHora < limite)
            .SumAsync(v => (double?)v.MontoTotal) ?? 0;
        var compras = await _db.Compras
            .Where(c => c.FechaHora < limite)
            .SumAsync(c => (double?)c.MontoTotal) ?? 0;
        return ventas - compras;
    }
}

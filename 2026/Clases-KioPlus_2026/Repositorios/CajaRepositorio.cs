using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

// La caja es un único registro con el saldo vigente. Lo mueven las ventas cobradas,
// los pagos de cuenta corriente y las compras a proveedor.
public class CajaRepositorio : ICajaRepositorio
{
    private readonly ApplicationDbContext _db;
    public CajaRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<double> SaldoActual() => (await ObtenerOCrear()).Monto;

    public async Task RegistrarIngreso(double monto)
    {
        if (monto <= 0) return;

        var caja = await ObtenerOCrear();
        caja.RegistrarIngreso(monto);
        await _db.SaveChangesAsync();
    }

    public async Task RegistrarEgreso(double monto)
    {
        if (monto <= 0) return;

        var caja = await ObtenerOCrear();
        caja.RegistrarEgreso(monto);
        await _db.SaveChangesAsync();
    }

    private async Task<Caja> ObtenerOCrear()
    {
        var caja = await _db.Cajas.FirstOrDefaultAsync();
        if (caja is not null) return caja;

        caja = new Caja { Monto = 0 };
        _db.Cajas.Add(caja);
        await _db.SaveChangesAsync();
        return caja;
    }
}

using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface ILoteRepositorio
{
    Task<bool> ProductoExiste(int idProducto);
    Task<IEnumerable<Lote>> ObtenerPorProducto(int idProducto);
    Task<Lote?> ObtenerPorId(int id);
    Task<Lote> Agregar(Lote lote);
    Task Actualizar(Lote lote);
    Task Eliminar(Lote lote);
}

public class LoteRepositorio : ILoteRepositorio
{
    private readonly ApplicationDbContext _db;
    public LoteRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<bool> ProductoExiste(int idProducto) =>
        await _db.Productos.AnyAsync(p => p.Id == idProducto);

    public async Task<IEnumerable<Lote>> ObtenerPorProducto(int idProducto) =>
        await _db.Lotes.Where(l => l.ProductoId == idProducto).ToListAsync();

    public async Task<Lote?> ObtenerPorId(int id) =>
        await _db.Lotes.FindAsync(id);

    public async Task<Lote> Agregar(Lote lote)
    {
        _db.Lotes.Add(lote);
        await _db.SaveChangesAsync();
        return lote;
    }

    public async Task Actualizar(Lote lote)
    {
        _db.Lotes.Update(lote);
        await _db.SaveChangesAsync();
    }

    public async Task Eliminar(Lote lote)
    {
        _db.Lotes.Remove(lote);
        await _db.SaveChangesAsync();
    }
}

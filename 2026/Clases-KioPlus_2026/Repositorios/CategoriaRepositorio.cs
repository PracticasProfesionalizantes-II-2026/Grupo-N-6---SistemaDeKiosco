using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public class CategoriaRepositorio : ICategoriaRepositorio
{
    private readonly ApplicationDbContext _db;
    public CategoriaRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Categoria>> ObtenerTodas() =>
        await _db.Categorias.Where(c => c.Activo).ToListAsync();

    public async Task<Categoria?> ObtenerPorId(int id) =>
        await _db.Categorias.FindAsync(id);

    public async Task<Categoria> Agregar(Categoria categoria)
    {
        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();
        return categoria;
    }

    public async Task Actualizar(Categoria categoria)
    {
        _db.Categorias.Update(categoria);
        await _db.SaveChangesAsync();
    }

    public async Task Eliminar(Categoria categoria)
    {
        categoria.Activo = false;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> TieneProductosActivos(int id) =>
        await _db.Productos.AnyAsync(p => p.CategoriaId == id && p.Activo);
}

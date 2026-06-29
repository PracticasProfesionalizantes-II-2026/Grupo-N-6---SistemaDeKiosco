using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface ICategoriaRepositorio
{
    Task<IEnumerable<Categoria>> ObtenerTodas();
    Task<Categoria?> ObtenerPorId(int id);
    Task<Categoria> Agregar(Categoria categoria);
    Task Actualizar(Categoria categoria);
    Task Eliminar(Categoria categoria);
}

public class CategoriaRepositorio : ICategoriaRepositorio
{
    private readonly ApplicationDbContext _db;
    public CategoriaRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Categoria>> ObtenerTodas() =>
        await _db.Categorias.ToListAsync();

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
        _db.Categorias.Remove(categoria);
        await _db.SaveChangesAsync();
    }
}

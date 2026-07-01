using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly ApplicationDbContext _db;
    public UsuarioRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Usuario>> ObtenerTodos() =>
        await _db.Usuarios.ToListAsync();

    public async Task<Usuario?> ObtenerPorId(int id) =>
        await _db.Usuarios.FindAsync(id);

    public async Task<Usuario> Agregar(Usuario usuario)
    {
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        return usuario;
    }

    public async Task Actualizar(Usuario usuario)
    {
        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync();
    }

    public async Task Eliminar(Usuario usuario)
    {
        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync();
    }
}

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

    public async Task<Usuario?> ObtenerPorNombreUsuario(string nombreUsuario) =>
        await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

    // Evita dos usuarios con el mismo nombre de acceso. Al editar se excluye el propio id.
    public async Task<bool> NombreUsuarioEnUso(string nombreUsuario, int? idExcluido) =>
        await _db.Usuarios.AnyAsync(u =>
            u.NombreUsuario == nombreUsuario && (!idExcluido.HasValue || u.Id != idExcluido.Value));

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

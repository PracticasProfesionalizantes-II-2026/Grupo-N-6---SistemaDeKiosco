using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public class NotificacionRepositorio : INotificacionRepositorio
{
    private readonly ApplicationDbContext _db;
    public NotificacionRepositorio(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Notificacion>> ObtenerTodas(Notificacion.TipoNotificacion? tipo)
    {
        var query = _db.Notificaciones.AsQueryable();
        if (tipo.HasValue)
            query = query.Where(n => n.Tipo == tipo.Value);
        return await query.ToListAsync();
    }
}

using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class NotificacionLogica : INotificacionLogica
{
    private readonly INotificacionRepositorio _repo;
    public NotificacionLogica(INotificacionRepositorio repo) => _repo = repo;

    public async Task<IEnumerable<NotificacionDto>> ObtenerTodas(string? tipo)
    {
        await Regenerar();

        Notificacion.TipoNotificacion? tipoEnum = tipo?.ToLower() switch
        {
            "stock" => Notificacion.TipoNotificacion.StockBajo,
            "vencimiento" => Notificacion.TipoNotificacion.ProximoVencimiento,
            _ => null
        };

        var notificaciones = await _repo.ObtenerTodas(tipoEnum);
        return notificaciones.Select(n => new NotificacionDto(n.Id, n.Tipo, n.Mensaje, n.FechaGeneracion));
    }

    // Arma los avisos a partir del stock actual y de los lotes próximos a vencer.
    private async Task Regenerar()
    {
        var ahora = DateTime.Now;
        var hoy = ahora.Date;
        var avisos = new List<Notificacion>();

        var criticos = await _repo.ObtenerProductosConStockCritico(ProductoLogica.UmbralStockCritico);
        foreach (var p in criticos)
        {
            avisos.Add(new Notificacion
            {
                Tipo = Notificacion.TipoNotificacion.StockBajo,
                Mensaje = $"Stock bajo: {p.Nombre} ({p.Marca}) — quedan {p.StockDisponible} unidades",
                FechaGeneracion = ahora
            });
        }

        var lotes = await _repo.ObtenerLotesConProducto();
        foreach (var l in lotes.Where(l => l.Producto is not null))
        {
            var dias = (l.FechaVencimiento.Date - hoy).Days;
            if (dias < 0 || dias > l.Producto.DiasAvisoVencimiento) continue;

            var cuando = dias == 0 ? "vence hoy" : $"vence en {dias} día(s)";
            avisos.Add(new Notificacion
            {
                Tipo = Notificacion.TipoNotificacion.ProximoVencimiento,
                Mensaje = $"Próximo vencimiento: {l.Producto.Nombre} — lote {l.NroLote} {cuando} ({l.Cantidad} unidades)",
                FechaGeneracion = ahora
            });
        }

        await _repo.Regenerar(avisos);
    }
}

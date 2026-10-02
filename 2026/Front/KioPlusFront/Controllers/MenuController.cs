using KioPlusFront.Filters;
using KioPlusFront.Models.Api;
using KioPlusFront.Services;
using Microsoft.AspNetCore.Mvc;

namespace KioPlusFront.Controllers;

[Autenticado]
public class MenuController : Controller
{
    private readonly INotificacionService _notificaciones;
    private readonly ICajaService _caja;

    public MenuController(INotificacionService notificaciones, ICajaService caja)
    {
        _notificaciones = notificaciones;
        _caja = caja;
    }

    public async Task<IActionResult> Index(bool sinPermiso = false)
    {
        if (sinPermiso)
            TempData["Error"] = "No tenés permisos para acceder a esa sección.";

        // Si los listados no se consultan, el aviso salta igual al entrar al menú
        var avisos = await _notificaciones.ObtenerTodasAsync();
        ViewBag.CantidadAvisos = avisos.Count;
        ViewBag.AvisosStock = avisos.Where(a => a.Tipo == TipoNotificacion.StockBajo).ToList();
        ViewBag.AvisosVencimiento = avisos.Where(a => a.Tipo == TipoNotificacion.ProximoVencimiento).ToList();

        // La caja no tiene pantalla propia: su saldo se muestra acá, solo a los
        // administradores, que es lo único que el alcance del sistema contempla.
        if (HttpContext.Session.ObtenerUsuario()?.EsAdministrador == true)
            ViewBag.SaldoCaja = await _caja.SaldoActualAsync();

        return View();
    }
}

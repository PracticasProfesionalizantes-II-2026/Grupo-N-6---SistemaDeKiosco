using KioPlusFront.Filters;
using KioPlusFront.Services;
using Microsoft.AspNetCore.Mvc;

namespace KioPlusFront.Controllers;

[Autenticado]
public class MenuController : Controller
{
    private readonly INotificacionService _notificaciones;

    public MenuController(INotificacionService notificaciones) => _notificaciones = notificaciones;

    public async Task<IActionResult> Index(bool sinPermiso = false)
    {
        if (sinPermiso)
            TempData["Error"] = "No tenés permisos para acceder a esa sección.";

        // La campana del menú muestra cuántos avisos hay pendientes
        var avisos = await _notificaciones.ObtenerTodasAsync();
        ViewBag.CantidadAvisos = avisos.Count;

        return View();
    }
}

using KioPlusFront.Filters;
using KioPlusFront.Services;
using Microsoft.AspNetCore.Mvc;

namespace KioPlusFront.Controllers;

[Autenticado]
public class NotificacionesController : Controller
{
    private readonly INotificacionService _notificaciones;

    public NotificacionesController(INotificacionService notificaciones) =>
        _notificaciones = notificaciones;

    // tipo admite "stock" o "vencimiento"; vacío trae todos los avisos
    public async Task<IActionResult> Index(string? tipo)
    {
        ViewBag.Tipo = tipo;
        var avisos = await _notificaciones.ObtenerTodasAsync(tipo);
        return View(avisos);
    }
}

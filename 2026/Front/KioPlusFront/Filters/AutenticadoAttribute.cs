using KioPlusFront.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KioPlusFront.Filters;

// Corta el acceso a cualquier pantalla si no hay sesión iniciada y
// deja el usuario logueado disponible para el layout.
public class AutenticadoAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var usuario = context.HttpContext.Session.ObtenerUsuario();

        if (usuario is null)
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        if (context.Controller is Controller controller)
            controller.ViewData["UsuarioActual"] = usuario;

        base.OnActionExecuting(context);
    }
}

// Restringe una acción a SuperAdmin y Administrador.
public class SoloAdministradorAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var usuario = context.HttpContext.Session.ObtenerUsuario();

        if (usuario is null)
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        if (!usuario.EsAdministrador)
        {
            context.Result = new RedirectToActionResult("Index", "Menu", new { sinPermiso = true });
            return;
        }

        if (context.Controller is Controller controller)
            controller.ViewData["UsuarioActual"] = usuario;

        base.OnActionExecuting(context);
    }
}

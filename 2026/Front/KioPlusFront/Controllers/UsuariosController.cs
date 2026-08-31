using KioPlusFront.Filters;
using KioPlusFront.Models.Api;
using KioPlusFront.Models.ViewModels;
using KioPlusFront.Services;
using Microsoft.AspNetCore.Mvc;

namespace KioPlusFront.Controllers;

// La gestión de usuarios queda reservada a SuperAdmin y Administrador.
[SoloAdministrador]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarios;

    public UsuariosController(IUsuarioService usuarios) => _usuarios = usuarios;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuarios = await _usuarios.ObtenerTodosAsync();
        return View(usuarios);
    }

    [HttpGet]
    public IActionResult Crear() => View(new UsuarioFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(UsuarioFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _usuarios.CrearAsync(new UsuarioCreateDto(
            modelo.NombreApellido, modelo.Telefono, modelo.NombreUsuario,
            modelo.ContraseniaUsuario, modelo.TipoUsuario, modelo.Estado));

        if (!resultado.Ok)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Exito"] = "Usuario agregado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(id);
        if (!usuario.Ok || usuario.Datos is null)
        {
            TempData["Error"] = usuario.Error ?? "No se encontró el usuario.";
            return RedirectToAction(nameof(Index));
        }

        var u = usuario.Datos;
        return View(new UsuarioFormViewModel
        {
            IdUsuario = u.IdUsuario,
            NombreApellido = u.NombreApellido,
            Telefono = u.Telefono,
            NombreUsuario = u.NombreUsuario,
            ContraseniaUsuario = u.ContraseniaUsuario,
            TipoUsuario = u.TipoUsuario,
            Estado = u.Estado
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(UsuarioFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _usuarios.ActualizarAsync(modelo.IdUsuario, new UsuarioCreateDto(
            modelo.NombreApellido, modelo.Telefono, modelo.NombreUsuario,
            modelo.ContraseniaUsuario, modelo.TipoUsuario, modelo.Estado));

        if (!resultado.Ok)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Exito"] = "Usuario actualizado.";
        return RedirectToAction(nameof(Index));
    }

    // Candado del listado: habilita o bloquea el acceso sin borrar el historial
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, bool estado)
    {
        var propio = HttpContext.Session.ObtenerUsuario();
        if (propio is not null && propio.IdUsuario == id && !estado)
        {
            TempData["Error"] = "No podés bloquear tu propio usuario.";
            return RedirectToAction(nameof(Index));
        }

        var resultado = await _usuarios.CambiarEstadoAsync(id, estado);
        if (resultado.Ok) TempData["Exito"] = estado ? "Usuario habilitado." : "Usuario bloqueado.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var propio = HttpContext.Session.ObtenerUsuario();
        if (propio is not null && propio.IdUsuario == id)
        {
            TempData["Error"] = "No podés eliminar tu propio usuario.";
            return RedirectToAction(nameof(Index));
        }

        var resultado = await _usuarios.EliminarAsync(id);
        if (resultado.Ok) TempData["Exito"] = "Usuario eliminado.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Index));
    }
}

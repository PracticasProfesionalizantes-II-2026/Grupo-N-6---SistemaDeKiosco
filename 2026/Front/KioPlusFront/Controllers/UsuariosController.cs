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

    // Solo el superadmin da de alta y baja administradores; el administrador
    // gestiona únicamente empleados.
    private bool PuedeGestionar(TipoDeUsuario tipo)
    {
        var propio = HttpContext.Session.ObtenerUsuario();
        if (propio is null) return false;
        if (propio.TipoUsuario == TipoDeUsuario.SuperAdmin) return true;
        return tipo == TipoDeUsuario.Empleado;
    }

    private const string SinPermisoSobreUsuario =
        "Solo el superadmin puede gestionar usuarios administradores.";

    private const string SuperAdminUnico =
        "El superadmin es único y viene con el sistema: no se puede crear otro.";

    [HttpGet]
    public IActionResult Crear() => View(new UsuarioFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(UsuarioFormViewModel modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.ContraseniaUsuario))
            ModelState.AddModelError(nameof(modelo.ContraseniaUsuario), "Ingresá la contraseña");

        if (!ModelState.IsValid) return View(modelo);

        if (modelo.TipoUsuario == TipoDeUsuario.SuperAdmin)
        {
            ModelState.AddModelError(string.Empty, SuperAdminUnico);
            return View(modelo);
        }

        if (!PuedeGestionar(modelo.TipoUsuario))
        {
            ModelState.AddModelError(string.Empty, SinPermisoSobreUsuario);
            return View(modelo);
        }

        var resultado = await _usuarios.CrearAsync(new UsuarioCreateDto(
            modelo.NombreApellido, modelo.Telefono, modelo.NombreUsuario,
            modelo.ContraseniaUsuario!, modelo.TipoUsuario, modelo.Estado));

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

        if (!PuedeGestionar(u.TipoUsuario))
        {
            TempData["Error"] = SinPermisoSobreUsuario;
            return RedirectToAction(nameof(Index));
        }

        return View(new UsuarioFormViewModel
        {
            IdUsuario = u.IdUsuario,
            NombreApellido = u.NombreApellido,
            Telefono = u.Telefono,
            NombreUsuario = u.NombreUsuario,
            TipoUsuario = u.TipoUsuario,
            Estado = u.Estado
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(UsuarioFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var actual = await _usuarios.ObtenerPorIdAsync(modelo.IdUsuario);
        var tipoActual = actual.Datos?.TipoUsuario ?? modelo.TipoUsuario;

        // Nadie asciende a superadmin: solo lo sigue siendo el que ya lo era
        if (modelo.TipoUsuario == TipoDeUsuario.SuperAdmin && tipoActual != TipoDeUsuario.SuperAdmin)
        {
            ModelState.AddModelError(string.Empty, SuperAdminUnico);
            return View(modelo);
        }

        if (!PuedeGestionar(tipoActual) || !PuedeGestionar(modelo.TipoUsuario))
        {
            ModelState.AddModelError(string.Empty, SinPermisoSobreUsuario);
            return View(modelo);
        }

        // Dejar el campo vacío significa conservar la contraseña actual
        var contrasenia = string.IsNullOrWhiteSpace(modelo.ContraseniaUsuario)
            ? actual.Datos?.ContraseniaUsuario ?? string.Empty
            : modelo.ContraseniaUsuario;

        var resultado = await _usuarios.ActualizarAsync(modelo.IdUsuario, new UsuarioCreateDto(
            modelo.NombreApellido, modelo.Telefono, modelo.NombreUsuario,
            contrasenia, modelo.TipoUsuario, modelo.Estado));

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

        var objetivo = await _usuarios.ObtenerPorIdAsync(id);
        if (objetivo.Datos is not null && !PuedeGestionar(objetivo.Datos.TipoUsuario))
        {
            TempData["Error"] = SinPermisoSobreUsuario;
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

        var objetivo = await _usuarios.ObtenerPorIdAsync(id);
        if (objetivo.Datos is not null && !PuedeGestionar(objetivo.Datos.TipoUsuario))
        {
            TempData["Error"] = SinPermisoSobreUsuario;
            return RedirectToAction(nameof(Index));
        }

        var resultado = await _usuarios.EliminarAsync(id);
        if (resultado.Ok) TempData["Exito"] = "Usuario eliminado.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Index));
    }
}

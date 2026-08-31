using KioPlusFront.Models.Api;
using KioPlusFront.Models.ViewModels;
using KioPlusFront.Services;
using Microsoft.AspNetCore.Mvc;

namespace KioPlusFront.Controllers;

public class AuthController : Controller
{
    private readonly IUsuarioService _usuarios;

    public AuthController(IUsuarioService usuarios) => _usuarios = usuarios;

    [HttpGet]
    public IActionResult Login()
    {
        // Si ya hay sesión abierta no tiene sentido volver a pedir credenciales
        if (HttpContext.Session.ObtenerUsuario() is not null)
            return RedirectToAction("Index", "Menu");

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _usuarios.LoginAsync(
            new LoginDto(modelo.NombreUsuario, modelo.Contrasenia));

        if (!resultado.Ok || resultado.Datos is null)
        {
            ModelState.AddModelError(string.Empty, resultado.Error ?? "No se pudo iniciar sesión.");
            return View(modelo);
        }

        var sesion = resultado.Datos;
        HttpContext.Session.GuardarUsuario(new UsuarioSesion(
            sesion.IdUsuario, sesion.NombreApellido, sesion.NombreUsuario, sesion.TipoUsuario));

        return RedirectToAction("Index", "Menu");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.CerrarSesion();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult RecuperarContrasenia() => View();
}

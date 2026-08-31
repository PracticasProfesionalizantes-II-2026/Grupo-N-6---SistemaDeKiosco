using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KioPlusFront.Models;

namespace KioPlusFront.Controllers;

public class HomeController : Controller
{
    // La raíz de la aplicación es el login; Home solo conserva la pantalla de error.
    public IActionResult Index() => RedirectToAction("Login", "Auth");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

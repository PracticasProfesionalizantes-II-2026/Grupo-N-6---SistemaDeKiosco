using KioPlusFront.Filters;
using KioPlusFront.Models.Api;
using KioPlusFront.Models.ViewModels;
using KioPlusFront.Services;
using Microsoft.AspNetCore.Mvc;

namespace KioPlusFront.Controllers;

[Autenticado]
public class ProveedoresController : Controller
{
    private readonly IProveedorService _proveedores;

    public ProveedoresController(IProveedorService proveedores) => _proveedores = proveedores;

    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda)
    {
        ViewBag.Busqueda = busqueda;
        var proveedores = await _proveedores.ObtenerTodosAsync(busqueda);
        return View(proveedores);
    }

    [HttpGet]
    public IActionResult Crear() => View(new ProveedorFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ProveedorFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _proveedores.CrearAsync(new ProveedorCreateDto(
            modelo.NombreRazonSocial, modelo.Telefono, modelo.Direccion,
            modelo.CorreoElectronico, modelo.Observaciones ?? string.Empty));

        if (!resultado.Ok)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Exito"] = "Proveedor agregado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var proveedor = await _proveedores.ObtenerPorIdAsync(id);
        if (!proveedor.Ok || proveedor.Datos is null)
        {
            TempData["Error"] = proveedor.Error ?? "No se encontró el proveedor.";
            return RedirectToAction(nameof(Index));
        }

        var p = proveedor.Datos;
        return View(new ProveedorFormViewModel
        {
            IdProveedor = p.IdProveedor,
            NombreRazonSocial = p.NombreRazonSocial,
            Telefono = p.Telefono,
            Direccion = p.Direccion,
            CorreoElectronico = p.CorreoElectronico,
            Observaciones = p.Observaciones
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ProveedorFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _proveedores.ActualizarAsync(modelo.IdProveedor, new ProveedorCreateDto(
            modelo.NombreRazonSocial, modelo.Telefono, modelo.Direccion,
            modelo.CorreoElectronico, modelo.Observaciones ?? string.Empty));

        if (!resultado.Ok)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Exito"] = "Proveedor actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SoloAdministrador]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _proveedores.EliminarAsync(id);
        if (resultado.Ok) TempData["Exito"] = "Proveedor eliminado.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Index));
    }
}

using KioPlusFront.Filters;
using KioPlusFront.Models.Api;
using KioPlusFront.Models.ViewModels;
using KioPlusFront.Services;
using Microsoft.AspNetCore.Mvc;

namespace KioPlusFront.Controllers;

[Autenticado]
public class CuentasCorrientesController : Controller
{
    private readonly ICuentaCorrienteService _cuentas;

    public CuentasCorrientesController(ICuentaCorrienteService cuentas) => _cuentas = cuentas;

    [HttpGet]
    public async Task<IActionResult> Index(ListadoCuentasCorrientesViewModel filtros)
    {
        filtros.Cuentas = await _cuentas.ObtenerTodasAsync(
            filtros.Nombre, filtros.Apellido, filtros.Dni,
            filtros.Estado, filtros.MontoMin, filtros.MontoMax);

        return View(filtros);
    }

    [HttpGet]
    public IActionResult Crear() => View(new CuentaCorrienteFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CuentaCorrienteFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _cuentas.CrearAsync(new CuentaCorrienteClienteCreateDto(
            modelo.Nombre, modelo.Apellido, modelo.Dni, modelo.Telefono,
            modelo.Direccion, modelo.CorreoElectronico, modelo.MontoAdeudado));

        if (!resultado.Ok)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Exito"] = "Cuenta corriente creada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var cuenta = await _cuentas.ObtenerPorIdAsync(id);
        if (!cuenta.Ok || cuenta.Datos is null)
        {
            TempData["Error"] = cuenta.Error ?? "No se encontró la cuenta corriente.";
            return RedirectToAction(nameof(Index));
        }

        var c = cuenta.Datos;
        return View(new CuentaCorrienteFormViewModel
        {
            IdCuentaCorrienteCliente = c.IdCuentaCorrienteCliente,
            Nombre = c.Nombre,
            Apellido = c.Apellido,
            Dni = c.Dni,
            Telefono = c.Telefono,
            Direccion = c.Direccion,
            CorreoElectronico = c.CorreoElectronico,
            MontoAdeudado = c.MontoAdeudado
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(CuentaCorrienteFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _cuentas.ActualizarAsync(modelo.IdCuentaCorrienteCliente,
            new CuentaCorrienteClienteCreateDto(
                modelo.Nombre, modelo.Apellido, modelo.Dni, modelo.Telefono,
                modelo.Direccion, modelo.CorreoElectronico, modelo.MontoAdeudado));

        if (!resultado.Ok)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Exito"] = "Cuenta corriente actualizada.";
        return RedirectToAction(nameof(Index));
    }

    // Icono $ del listado: cancela total o parcialmente la deuda del cliente
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarPago(int id, double monto)
    {
        if (monto <= 0)
        {
            TempData["Error"] = "El monto del pago debe ser mayor a 0.";
            return RedirectToAction(nameof(Index));
        }

        var resultado = await _cuentas.RegistrarPagoAsync(id, monto);
        if (resultado.Ok) TempData["Exito"] = $"Pago de ${monto:N2} registrado.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SoloAdministrador]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _cuentas.EliminarAsync(id);
        if (resultado.Ok) TempData["Exito"] = "Cuenta corriente eliminada.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Index));
    }
}

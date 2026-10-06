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
    private readonly IVentaService _ventas;

    public CuentasCorrientesController(ICuentaCorrienteService cuentas, IVentaService ventas)
    {
        _cuentas = cuentas;
        _ventas = ventas;
    }

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
        if (id == CuentaCorrienteService.IdConsumidorFinal)
        {
            TempData["Error"] = "La cuenta Consumidor Final no se puede modificar.";
            return RedirectToAction(nameof(Index));
        }

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
        if (modelo.IdCuentaCorrienteCliente == CuentaCorrienteService.IdConsumidorFinal)
        {
            TempData["Error"] = "La cuenta Consumidor Final no se puede modificar.";
            return RedirectToAction(nameof(Index));
        }

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

    // Ventas que el cliente todavía adeuda, con el detalle de cada una
    [HttpGet]
    public async Task<IActionResult> Deuda(int id)
    {
        var cuenta = await _cuentas.ObtenerPorIdAsync(id);
        if (!cuenta.Ok || cuenta.Datos is null)
        {
            TempData["Error"] = cuenta.Error ?? "No se encontró la cuenta corriente.";
            return RedirectToAction(nameof(Index));
        }

        var c = cuenta.Datos;
        return View(new DeudaClienteViewModel
        {
            IdCuentaCorrienteCliente = c.IdCuentaCorrienteCliente,
            Cliente = $"{c.Nombre} {c.Apellido}",
            MontoAdeudado = c.MontoAdeudado,
            Ventas = await _cuentas.ObtenerVentasAdeudadasAsync(id)
        });
    }

    // Cobra una venta puntual: le pone la fecha de pago y la saca de la lista
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PagarVenta(int id, int idVenta, DateTime? fechaPago)
    {
        var resultado = await _ventas.RegistrarPagoAsync(idVenta, fechaPago);
        if (resultado.Ok) TempData["Exito"] = $"Venta #{idVenta} pagada.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Deuda), new { id });
    }

    // Cancela la deuda completa del cliente de una sola vez
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PagarTodo(int id, DateTime? fechaPago)
    {
        var resultado = await _cuentas.PagarDeudaTotalAsync(id, fechaPago);
        if (resultado.Ok) TempData["Exito"] = "Deuda cancelada por completo.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Deuda), new { id });
    }

    // Icono $ del listado: registra un pago a cuenta por un monto libre
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

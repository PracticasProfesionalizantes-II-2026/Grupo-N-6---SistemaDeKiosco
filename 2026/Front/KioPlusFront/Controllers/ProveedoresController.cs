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
    private readonly IProductoService _productos;

    public ProveedoresController(IProveedorService proveedores, IProductoService productos)
    {
        _proveedores = proveedores;
        _productos = productos;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda)
    {
        ViewBag.Busqueda = busqueda;
        var proveedores = await _proveedores.ObtenerTodosAsync(busqueda);
        return View(proveedores);
    }

    // Los productos asociados al proveedor: la misma tabla que se edita desde
    // Stock, leída desde el otro lado.
    [HttpGet]
    public async Task<IActionResult> Productos(int id)
    {
        var proveedor = await _proveedores.ObtenerPorIdAsync(id);
        if (!proveedor.Ok || proveedor.Datos is null)
        {
            TempData["Error"] = proveedor.Error ?? "No se encontró el proveedor.";
            return RedirectToAction(nameof(Index));
        }

        return View(await ArmarProductosAsync(id, proveedor.Datos.NombreRazonSocial));
    }

    private async Task<ProductosDelProveedorViewModel> ArmarProductosAsync(int id, string nombre)
    {
        var asociados = await _proveedores.ObtenerProductosDelProveedorAsync(id);
        var todos = await _productos.ObtenerTodosAsync();

        return new ProductosDelProveedorViewModel
        {
            IdProveedor = id,
            NombreProveedor = nombre,
            Productos = asociados,
            // No se ofrecen los que ya están asociados: se asocia una sola vez
            ProductosDisponibles = todos
                .Where(p => asociados.All(a => a.IdProducto != p.IdProducto))
                .ToList()
        };
    }

    // Asocia un producto a este proveedor. Es la misma tabla que se edita desde
    // Stock, así que alcanza con llamar al mismo endpoint del lado del producto.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsociarProducto(int id, int idProducto, double precioCompra)
    {
        if (idProducto <= 0)
        {
            TempData["Error"] = "Elegí un producto de la lista.";
            return RedirectToAction(nameof(Productos), new { id });
        }

        if (precioCompra <= 0)
        {
            TempData["Error"] = "El precio de compra debe ser mayor a 0.";
            return RedirectToAction(nameof(Productos), new { id });
        }

        var resultado = await _proveedores.AsociarProductoAsync(idProducto,
            new ProductoProveedorCreateDto(id, precioCompra));

        if (resultado.Ok) TempData["Exito"] = "Producto asociado al proveedor.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Productos), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPrecio(int id, int idProducto, int idAsociacion, double precioCompra)
    {
        if (precioCompra <= 0)
        {
            TempData["Error"] = "El precio de compra debe ser mayor a 0.";
            return RedirectToAction(nameof(Productos), new { id });
        }

        var resultado = await _proveedores.ActualizarPrecioAsync(idProducto, idAsociacion, precioCompra);
        if (resultado.Ok) TempData["Exito"] = "Precio actualizado.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Productos), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesasociarProducto(int id, int idProducto, int idAsociacion)
    {
        var resultado = await _proveedores.DesasociarProductoAsync(idProducto, idAsociacion);
        if (resultado.Ok) TempData["Exito"] = "Asociación eliminada.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Productos), new { id });
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
            modelo.CorreoElectronico, modelo.Observaciones));

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
            modelo.CorreoElectronico, modelo.Observaciones));

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

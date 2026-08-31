using KioPlusFront.Filters;
using KioPlusFront.Models.Api;
using KioPlusFront.Models.ViewModels;
using KioPlusFront.Services;
using Microsoft.AspNetCore.Mvc;

namespace KioPlusFront.Controllers;

// Compras a proveedor: es la vía por la que entra mercadería al stock.
[Autenticado]
public class ComprasController : Controller
{
    private const string ClaveCarrito = "kioplus.carrito.compra";

    private readonly ICompraService _compras;
    private readonly IProductoService _productos;
    private readonly IProveedorService _proveedores;

    public ComprasController(
        ICompraService compras,
        IProductoService productos,
        IProveedorService proveedores)
    {
        _compras = compras;
        _productos = productos;
        _proveedores = proveedores;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ListadoComprasViewModel filtros)
    {
        filtros.Compras = await _compras.ObtenerTodasAsync(
            filtros.FechaDesde, filtros.FechaHasta, filtros.IdProveedor);
        filtros.Proveedores = await _proveedores.ObtenerTodosAsync();
        return View(filtros);
    }

    [HttpGet]
    public async Task<IActionResult> Nueva()
    {
        var modelo = await ArmarNuevaCompraAsync();
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarItem(int idProducto, int cantidad, double precioUnitario)
    {
        if (cantidad <= 0 || precioUnitario <= 0)
        {
            TempData["Error"] = "La cantidad y el precio deben ser mayores a 0.";
            return RedirectToAction(nameof(Nueva));
        }

        var producto = (await _productos.ObtenerTodosAsync())
            .FirstOrDefault(p => p.IdProducto == idProducto);

        if (producto is null)
        {
            TempData["Error"] = "Elegí un producto de la lista.";
            return RedirectToAction(nameof(Nueva));
        }

        var carrito = LeerCarrito();
        var existente = carrito.FirstOrDefault(i => i.IdProducto == idProducto);

        if (existente is not null)
        {
            existente.Cantidad += cantidad;
            existente.PrecioUnitario = precioUnitario;
        }
        else
        {
            carrito.Add(new ItemCompra
            {
                IdProducto = producto.IdProducto,
                Producto = $"{producto.Nombre} ({producto.Marca})",
                Cantidad = cantidad,
                PrecioUnitario = precioUnitario
            });
        }

        GuardarCarrito(carrito);
        return RedirectToAction(nameof(Nueva));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult QuitarItem(int idProducto)
    {
        var carrito = LeerCarrito();
        carrito.RemoveAll(i => i.IdProducto == idProducto);
        GuardarCarrito(carrito);
        return RedirectToAction(nameof(Nueva));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar()
    {
        HttpContext.Session.Remove(ClaveCarrito);
        return RedirectToAction(nameof(Index));
    }

    // Registra la compra y sus renglones. Cada renglón ingresa stock al producto.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar(int idProveedor, DateTime fechaHora)
    {
        var carrito = LeerCarrito();
        if (carrito.Count == 0)
        {
            TempData["Error"] = "Agregá al menos un producto antes de registrar la compra.";
            return RedirectToAction(nameof(Nueva));
        }

        if (idProveedor <= 0)
        {
            TempData["Error"] = "Elegí el proveedor de la compra.";
            return RedirectToAction(nameof(Nueva));
        }

        var creada = await _compras.CrearAsync(new CompraCreateDto(
            fechaHora == default ? DateTime.Now : fechaHora, idProveedor));

        if (!creada.Ok || creada.Datos is null)
        {
            TempData["Error"] = creada.Error ?? "No se pudo registrar la compra.";
            return RedirectToAction(nameof(Nueva));
        }

        var idCompra = creada.Datos.IdCompraProveedor;

        foreach (var item in carrito)
        {
            var detalle = await _compras.AgregarDetalleAsync(idCompra,
                new DetalleCompraCreateDto(item.IdProducto, item.Cantidad, item.PrecioUnitario));

            if (!detalle.Ok)
            {
                await _compras.EliminarAsync(idCompra);
                TempData["Error"] = detalle.Error ?? "No se pudo cargar uno de los productos.";
                return RedirectToAction(nameof(Nueva));
            }
        }

        HttpContext.Session.Remove(ClaveCarrito);
        TempData["Exito"] = $"Compra #{idCompra} registrada. El stock ya fue actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var compra = await _compras.ObtenerPorIdAsync(id);
        if (!compra.Ok || compra.Datos is null)
        {
            TempData["Error"] = compra.Error ?? "No se encontró la compra.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Compra = compra.Datos;
        var detalles = await _compras.ObtenerDetallesAsync(id);
        return View(detalles);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SoloAdministrador]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _compras.EliminarAsync(id);
        if (resultado.Ok) TempData["Exito"] = "Compra eliminada.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Index));
    }

    private List<ItemCompra> LeerCarrito() =>
        HttpContext.Session.ObtenerObjeto<List<ItemCompra>>(ClaveCarrito) ?? new List<ItemCompra>();

    private void GuardarCarrito(List<ItemCompra> carrito) =>
        HttpContext.Session.GuardarObjeto(ClaveCarrito, carrito);

    private async Task<NuevaCompraViewModel> ArmarNuevaCompraAsync() => new()
    {
        Items = LeerCarrito(),
        Productos = await _productos.ObtenerTodosAsync(),
        Proveedores = await _proveedores.ObtenerTodosAsync()
    };
}

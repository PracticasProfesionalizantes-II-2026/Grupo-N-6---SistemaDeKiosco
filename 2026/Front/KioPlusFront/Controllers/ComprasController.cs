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
            Periodos.DesdeEfectiva(filtros), Periodos.HastaEfectiva(filtros),
            filtros.IdProveedor);
        filtros.Proveedores = await _proveedores.ObtenerTodosAsync();
        return View(filtros);
    }

    // idProductoNuevo llega cuando el usuario dio de alta un producto sin salir de
    // esta compra: los renglones siguen en sesión y el producto vuelve preseleccionado.
    [HttpGet]
    public async Task<IActionResult> Nueva(int? idProductoNuevo)
    {
        var modelo = await ArmarNuevaCompraAsync();
        modelo.IdProductoNuevo = idProductoNuevo;
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarItem(
        int idProducto, int cantidad, double precioUnitario,
        string? nroLote, DateTime? fechaVencimiento)
    {
        if (cantidad <= 0 || precioUnitario <= 0)
        {
            TempData["Error"] = "La cantidad y el precio deben ser mayores a 0.";
            return RedirectToAction(nameof(Nueva));
        }

        if (fechaVencimiento?.Date < DateTime.Today)
        {
            TempData["Error"] = "La fecha de vencimiento no puede ser anterior a hoy.";
            return RedirectToAction(nameof(Nueva));
        }

        var producto = (await _productos.ObtenerTodosAsync())
            .FirstOrDefault(p => p.IdProducto == idProducto);

        if (producto is null)
        {
            TempData["Error"] = "Elegí un producto de la lista.";
            return RedirectToAction(nameof(Nueva));
        }

        // Cada renglón ingresa su propio lote, así que dos renglones del mismo
        // producto con vencimientos distintos no se fusionan.
        var carrito = LeerCarrito();
        carrito.Add(new ItemCompra
        {
            IdProducto = producto.IdProducto,
            Producto = $"{producto.Nombre} ({producto.Marca})",
            Cantidad = cantidad,
            PrecioUnitario = precioUnitario,
            NroLote = string.IsNullOrWhiteSpace(nroLote) ? null : nroLote.Trim(),
            FechaVencimiento = fechaVencimiento
        });

        GuardarCarrito(carrito);
        return RedirectToAction(nameof(Nueva));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult QuitarItem(int indice)
    {
        var carrito = LeerCarrito();
        if (indice >= 0 && indice < carrito.Count)
        {
            carrito.RemoveAt(indice);
            GuardarCarrito(carrito);
        }
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

        // El formulario solo elige el día; la hora es la del momento del registro,
        // así el listado no muestra todas las compras a las 00:00.
        var ahora = DateTime.Now;
        var momento = fechaHora == default
            ? ahora
            : fechaHora.Date.Add(fechaHora.Date == ahora.Date ? ahora.TimeOfDay : new TimeSpan(9, 0, 0));

        var creada = await _compras.CrearAsync(new CompraCreateDto(momento, idProveedor));

        if (!creada.Ok || creada.Datos is null)
        {
            TempData["Error"] = creada.Error ?? "No se pudo registrar la compra.";
            return RedirectToAction(nameof(Nueva));
        }

        var idCompra = creada.Datos.IdCompraProveedor;

        foreach (var item in carrito)
        {
            var detalle = await _compras.AgregarDetalleAsync(idCompra,
                new DetalleCompraCreateDto(
                    item.IdProducto, item.Cantidad, item.PrecioUnitario,
                    item.NroLote, item.FechaVencimiento));

            if (!detalle.Ok)
            {
                await _compras.EliminarAsync(idCompra);
                TempData["Error"] = detalle.Error ?? "No se pudo cargar uno de los productos.";
                return RedirectToAction(nameof(Nueva));
            }
        }

        HttpContext.Session.Remove(ClaveCarrito);
        TempData["Exito"] = $"Compra #{idCompra} registrada. Cada renglón ingresó como un lote nuevo.";
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

using KioPlusFront.Filters;
using KioPlusFront.Models.Api;
using KioPlusFront.Models.ViewModels;
using KioPlusFront.Services;
using Microsoft.AspNetCore.Mvc;

namespace KioPlusFront.Controllers;

[Autenticado]
public class VentasController : Controller
{
    private const string ClaveCarrito = "kioplus.carrito.venta";

    private readonly IVentaService _ventas;
    private readonly IProductoService _productos;
    private readonly ICuentaCorrienteService _cuentas;
    private readonly IUsuarioService _usuarios;

    public VentasController(
        IVentaService ventas,
        IProductoService productos,
        ICuentaCorrienteService cuentas,
        IUsuarioService usuarios)
    {
        _ventas = ventas;
        _productos = productos;
        _cuentas = cuentas;
        _usuarios = usuarios;
    }

    public IActionResult Index() => View();

    // ---------- Nueva venta ----------

    [HttpGet]
    public async Task<IActionResult> Nueva()
    {
        var modelo = await ArmarNuevaVentaAsync();
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarItem(int idProducto, int cantidad)
    {
        if (cantidad <= 0)
        {
            TempData["Error"] = "La cantidad debe ser mayor a 0.";
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
        var yaEnCarrito = existente?.Cantidad ?? 0;

        // Se valida contra el stock antes de llamar a la API para no dejar
        // la venta a medio armar con un renglón rechazado.
        if (producto.StockDisponible < yaEnCarrito + cantidad)
        {
            TempData["Error"] =
                $"Stock insuficiente para {producto.Nombre}: quedan {producto.StockDisponible} unidades.";
            return RedirectToAction(nameof(Nueva));
        }

        if (existente is not null)
        {
            existente.Cantidad += cantidad;
        }
        else
        {
            carrito.Add(new ItemCarrito
            {
                IdProducto = producto.IdProducto,
                Producto = $"{producto.Nombre} ({producto.Marca})",
                Cantidad = cantidad,
                PrecioUnitario = producto.PrecioVenta
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

    // Arma la venta completa: cabecera, renglones y cierre. Si algún renglón
    // falla se borra la venta para no dejar una cabecera huérfana.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar(FormaDePago formaPago, int idCliente)
    {
        var carrito = LeerCarrito();
        if (carrito.Count == 0)
        {
            TempData["Error"] = "Agregá al menos un producto antes de finalizar la venta.";
            return RedirectToAction(nameof(Nueva));
        }

        var usuario = HttpContext.Session.ObtenerUsuario()!;
        var cliente = idCliente <= 0 ? CuentaCorrienteService.IdConsumidorFinal : idCliente;

        if (formaPago == FormaDePago.CuentaCorriente && cliente == CuentaCorrienteService.IdConsumidorFinal)
        {
            TempData["Error"] = "Una venta en cuenta corriente necesita un cliente registrado.";
            return RedirectToAction(nameof(Nueva));
        }

        var ahora = DateTime.Now;
        var creada = await _ventas.CrearAsync(new VentaCreateDto(
            ahora, usuario.IdUsuario, cliente, formaPago,
            formaPago == FormaDePago.PagadoAlMomento ? ahora : default));

        if (!creada.Ok || creada.Datos is null)
        {
            TempData["Error"] = creada.Error ?? "No se pudo registrar la venta.";
            return RedirectToAction(nameof(Nueva));
        }

        var idVenta = creada.Datos.IdVenta;

        foreach (var item in carrito)
        {
            var detalle = await _ventas.AgregarDetalleAsync(
                idVenta, new DetalleVentaCreateDto(item.IdProducto, item.Cantidad));

            if (!detalle.Ok)
            {
                await _ventas.EliminarAsync(idVenta);
                TempData["Error"] = detalle.Error ?? "No se pudo cargar uno de los productos.";
                return RedirectToAction(nameof(Nueva));
            }
        }

        var cierre = await _ventas.FinalizarAsync(idVenta);
        if (!cierre.Ok)
        {
            TempData["Error"] = cierre.Error ?? "No se pudo cerrar la venta.";
            return RedirectToAction(nameof(Nueva));
        }

        HttpContext.Session.Remove(ClaveCarrito);
        TempData["Exito"] = $"Venta #{idVenta} registrada por ${carrito.Sum(i => i.Subtotal):N2}.";
        return RedirectToAction(nameof(Listado));
    }

    // ---------- Listado ----------

    [HttpGet]
    public async Task<IActionResult> Listado(ListadoVentasViewModel filtros)
    {
        filtros.Ventas = await _ventas.ObtenerTodasAsync(
            filtros.FechaDesde, filtros.FechaHasta, filtros.IdUsuario,
            filtros.IdCliente, filtros.ImporteMayorA, filtros.ImporteMenorA);

        filtros.Vendedores = await _usuarios.ObtenerTodosAsync();
        filtros.Clientes = await _cuentas.ObtenerTodasAsync();

        return View(filtros);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var venta = await _ventas.ObtenerPorIdAsync(id);
        if (!venta.Ok || venta.Datos is null)
        {
            TempData["Error"] = venta.Error ?? "No se encontró la venta.";
            return RedirectToAction(nameof(Listado));
        }

        ViewBag.Venta = venta.Datos;
        var detalles = await _ventas.ObtenerDetallesAsync(id);
        return View(detalles);
    }

    // ---------- Productos más vendidos ----------

    [HttpGet]
    public async Task<IActionResult> MasVendidos(MasVendidosViewModel modelo)
    {
        if (modelo.FechaHasta < modelo.FechaDesde)
            ModelState.AddModelError(nameof(modelo.FechaHasta), "La fecha final no puede ser anterior a la inicial.");

        if (ModelState.IsValid)
        {
            if (modelo.Criterio == "monto")
                modelo.PorMonto = await _productos.MasVendidosPorMontoAsync(
                    modelo.FechaDesde, modelo.FechaHasta, modelo.Limite);
            else
                modelo.PorCantidad = await _productos.MasVendidosPorCantidadAsync(
                    modelo.FechaDesde, modelo.FechaHasta, modelo.Limite);
        }

        return View(modelo);
    }

    // ---------- Carrito en sesión ----------

    private List<ItemCarrito> LeerCarrito() =>
        HttpContext.Session.ObtenerObjeto<List<ItemCarrito>>(ClaveCarrito) ?? new List<ItemCarrito>();

    private void GuardarCarrito(List<ItemCarrito> carrito) =>
        HttpContext.Session.GuardarObjeto(ClaveCarrito, carrito);

    private async Task<NuevaVentaViewModel> ArmarNuevaVentaAsync() => new()
    {
        Items = LeerCarrito(),
        Productos = await _productos.ObtenerTodosAsync(),
        Clientes = await _cuentas.ObtenerTodasAsync()
    };
}

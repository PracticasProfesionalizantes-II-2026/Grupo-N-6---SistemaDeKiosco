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

    // idProductoNuevo llega cuando el usuario dio de alta un producto sin salir de
    // esta venta: el carrito sigue en sesión y el producto vuelve preseleccionado.
    [HttpGet]
    public async Task<IActionResult> Nueva(int? idProductoNuevo)
    {
        var modelo = await ArmarNuevaVentaAsync();
        modelo.IdProductoNuevo = idProductoNuevo;
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

    // Cambio de cantidad de un producto ya cargado, antes de finalizar la venta
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarItem(int idProducto, int cantidad)
    {
        var carrito = LeerCarrito();
        var item = carrito.FirstOrDefault(i => i.IdProducto == idProducto);
        if (item is null) return RedirectToAction(nameof(Nueva));

        if (cantidad <= 0)
        {
            TempData["Error"] = "La cantidad debe ser mayor a 0.";
            return RedirectToAction(nameof(Nueva));
        }

        var producto = (await _productos.ObtenerTodosAsync())
            .FirstOrDefault(p => p.IdProducto == idProducto);

        if (producto is not null && producto.StockDisponible < cantidad)
        {
            TempData["Error"] =
                $"Stock insuficiente para {producto.Nombre}: quedan {producto.StockDisponible} unidades.";
            return RedirectToAction(nameof(Nueva));
        }

        item.Cantidad = cantidad;
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
            formaPago == FormaDePago.PagadoAlMomento ? ahora : null));

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
            // Se deshace la venta a medias: eliminarla devuelve las unidades a
            // los lotes de los que salieron. Sin esto quedaba stock descontado
            // por una venta que nunca se cerró, y el carrito seguía en sesión,
            // así que un segundo intento la registraba dos veces.
            await _ventas.EliminarAsync(idVenta);
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
            Periodos.DesdeEfectiva(filtros), Periodos.HastaEfectiva(filtros),
            filtros.IdUsuario, filtros.IdCliente,
            filtros.ImporteMayorA, filtros.ImporteMenorA);

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

    // Pantalla única de edición: la cabecera y los renglones de la venta se
    // corrigen desde acá. Cada cambio de renglón mueve el stock por los lotes en
    // el momento, así no queda una venta a medio aplicar.
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var modelo = await ArmarEdicionAsync(id);
        if (modelo is null) return RedirectToAction(nameof(Listado));

        return View(modelo);
    }

    private async Task<EditarVentaViewModel?> ArmarEdicionAsync(int id)
    {
        var venta = await _ventas.ObtenerPorIdAsync(id);
        if (!venta.Ok || venta.Datos is null)
        {
            TempData["Error"] = venta.Error ?? "No se encontró la venta.";
            return null;
        }

        var v = venta.Datos;
        return new EditarVentaViewModel
        {
            IdVenta = v.IdVenta,
            FechaHora = v.FechaHora,
            MontoTotal = v.MontoTotal,
            IdUsuario = v.IdUsuario,
            Finalizada = v.Finalizada,
            Estado = v.Estado,
            MontoPagado = v.MontoPagado,
            FormaPago = v.FormaPago,
            IdCliente = v.IdCuentaCorrienteCliente,
            Clientes = await _cuentas.ObtenerTodasAsync(),
            Detalles = await _ventas.ObtenerDetallesAsync(id),
            Productos = await _productos.ObtenerTodosAsync()
        };
    }

    // Suma un producto a una venta ya registrada
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarDetalle(int id, int idProducto, int cantidad)
    {
        if (idProducto <= 0)
        {
            TempData["Error"] = "Elegí un producto de la lista.";
            return RedirectToAction(nameof(Editar), new { id });
        }

        if (cantidad <= 0)
        {
            TempData["Error"] = "La cantidad debe ser mayor a 0.";
            return RedirectToAction(nameof(Editar), new { id });
        }

        var resultado = await _ventas.AgregarDetalleAsync(id, new DetalleVentaCreateDto(idProducto, cantidad));
        if (resultado.Ok) TempData["Exito"] = "Producto agregado a la venta.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Editar), new { id });
    }

    // Solo se corrigen los datos de cabecera. Los productos de la venta se
    // ajustan desde su detalle, para que el stock siga los mismos caminos.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarVentaViewModel modelo)
    {
        if (modelo.FormaPago == FormaDePago.CuentaCorriente &&
            modelo.IdCliente == CuentaCorrienteService.IdConsumidorFinal)
        {
            ModelState.AddModelError(string.Empty,
                "Una venta en cuenta corriente necesita un cliente registrado.");
        }

        if (!ModelState.IsValid)
        {
            return View(await ArmarEdicionAsync(modelo.IdVenta) ?? modelo);
        }

        var resultado = await _ventas.ActualizarAsync(modelo.IdVenta, new VentaCreateDto(
            modelo.FechaHora, modelo.IdUsuario, modelo.IdCliente, modelo.FormaPago,
            modelo.FormaPago == FormaDePago.PagadoAlMomento ? modelo.FechaHora : null));

        if (!resultado.Ok)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Editar), new { id = modelo.IdVenta });
        }

        TempData["Exito"] = $"Venta #{modelo.IdVenta} actualizada.";
        return RedirectToAction(nameof(Listado));
    }

    // Anular la venta devuelve la mercadería a sus lotes y deshace el impacto
    // en la caja o en la cuenta corriente del cliente.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SoloAdministrador]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _ventas.EliminarAsync(id);
        if (resultado.Ok) TempData["Exito"] = $"Venta #{id} anulada. El stock volvió a sus lotes.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Listado));
    }

    // Corrección de un renglón de una venta ya registrada. El stock se mueve
    // por los lotes: subir la cantidad consume FEFO y bajarla devuelve unidades.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarDetalle(int id, int idDetalle, int cantidad)
    {
        if (cantidad <= 0)
        {
            TempData["Error"] = "La cantidad debe ser mayor a 0.";
            return RedirectToAction(nameof(Editar), new { id });
        }

        var resultado = await _ventas.ActualizarDetalleAsync(id, idDetalle, new DetalleVentaUpdateDto(cantidad));
        if (resultado.Ok) TempData["Exito"] = "Renglón actualizado.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Editar), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarDetalle(int id, int idDetalle)
    {
        var resultado = await _ventas.EliminarDetalleAsync(id, idDetalle);
        if (resultado.Ok) TempData["Exito"] = "Renglón quitado de la venta.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Editar), new { id });
    }

    // ---------- Productos más vendidos ----------

    [HttpGet]
    [SoloAdministrador]
    public async Task<IActionResult> MasVendidos(MasVendidosViewModel modelo)
    {
        var desde = Periodos.DesdeEfectiva(modelo);
        var hasta = Periodos.HastaEfectiva(modelo);

        if (desde is not null && hasta is not null && hasta < desde)
            ModelState.AddModelError(nameof(modelo.FechaHasta), "La fecha final no puede ser anterior a la inicial.");

        if (ModelState.IsValid)
        {
            if (modelo.Criterio == "monto")
                modelo.PorMonto = await _productos.MasVendidosPorMontoAsync(desde, hasta, modelo.Limite);
            else
                modelo.PorCantidad = await _productos.MasVendidosPorCantidadAsync(desde, hasta, modelo.Limite);
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

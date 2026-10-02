using KioPlusFront.Filters;
using KioPlusFront.Models.Api;
using KioPlusFront.Models.ViewModels;
using KioPlusFront.Services;
using Microsoft.AspNetCore.Mvc;

namespace KioPlusFront.Controllers;

[Autenticado]
public class StockController : Controller
{
    private readonly IProductoService _productos;
    private readonly ICategoriaService _categorias;
    private readonly ILoteService _lotes;
    private readonly IProveedorService _proveedores;

    public StockController(
        IProductoService productos,
        ICategoriaService categorias,
        ILoteService lotes,
        IProveedorService proveedores)
    {
        _productos = productos;
        _categorias = categorias;
        _lotes = lotes;
        _proveedores = proveedores;
    }

    public IActionResult Index() => View();

    // ---------- Productos ----------

    [HttpGet]
    public async Task<IActionResult> Productos(ListadoProductosViewModel filtros)
    {
        filtros.Productos = await _productos.ObtenerTodosAsync(
            filtros.Nombre, filtros.IdCategoria, filtros.Marca);
        filtros.Categorias = await _categorias.ObtenerTodasAsync();
        return View(filtros);
    }

    // volverA llega desde el botón "Nuevo Producto" de una venta o una compra en
    // curso: es la pantalla a la que hay que devolver al usuario al terminar el alta.
    [HttpGet]
    public async Task<IActionResult> CrearProducto(string? volverA)
    {
        var modelo = new ProductoFormViewModel { VolverA = volverA };
        await CompletarListasAsync(modelo);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearProducto(ProductoFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            await CompletarListasAsync(modelo);
            return View(modelo);
        }

        var resultado = await _productos.CrearAsync(new ProductoCreateDto(
            modelo.Nombre, modelo.Marca, modelo.IdCategoria,
            modelo.PrecioVenta, modelo.DiasAvisoVencimiento));

        if (!resultado.Ok || resultado.Datos is null)
        {
            ModelState.AddModelError(string.Empty, resultado.Error ?? "No se pudo crear el producto.");
            await CompletarListasAsync(modelo);
            return View(modelo);
        }

        var idProducto = resultado.Datos.IdProducto;
        var avisos = new List<string>();

        // El stock nace del primer lote. Sin lote el producto queda en cero.
        if (modelo.CantidadInicial > 0)
        {
            var lote = await _lotes.CrearAsync(idProducto, new LoteCreateDto(
                string.IsNullOrWhiteSpace(modelo.NroLote) ? "L1" : modelo.NroLote,
                modelo.FechaVencimiento, modelo.CantidadInicial));

            if (!lote.Ok) avisos.Add($"El producto se creó pero no se pudo cargar el lote: {lote.Error}");
        }

        if (modelo.IdProveedor > 0)
        {
            var asociacion = await _proveedores.AsociarProductoAsync(idProducto,
                new ProductoProveedorCreateDto(modelo.IdProveedor, modelo.PrecioCompra));

            if (!asociacion.Ok) avisos.Add($"No se pudo asociar el proveedor: {asociacion.Error}");
        }

        if (avisos.Count > 0) TempData["Error"] = string.Join(" ", avisos);

        TempData["Exito"] = modelo.CantidadInicial > 0
            ? "Producto agregado con su primer lote."
            : "Producto agregado. Cargá una compra o un lote para darle stock.";

        return VolverAlOrigen(modelo.VolverA, idProducto);
    }

    // Alta rápida de categoría sin salir del alta de producto: no hay redirect,
    // así que el formulario vuelve con todo lo que el usuario ya había escrito y
    // con la categoría nueva ya seleccionada.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCategoriaRapida(ProductoFormViewModel modelo)
    {
        ModelState.Clear();

        if (string.IsNullOrWhiteSpace(modelo.NuevaCategoria))
        {
            TempData["Error"] = "Escribí el nombre de la categoría que querés crear.";
            await CompletarListasAsync(modelo);
            return View(nameof(CrearProducto), modelo);
        }

        var resultado = await _categorias.CrearAsync(
            new CategoriaCreateDto(modelo.NuevaCategoria.Trim(), modelo.NuevaCategoria.Trim()));

        if (!resultado.Ok)
        {
            TempData["Error"] = resultado.Error;
            await CompletarListasAsync(modelo);
            return View(nameof(CrearProducto), modelo);
        }

        await CompletarListasAsync(modelo);

        // Queda seleccionada la categoría recién creada
        var creada = modelo.Categorias.FirstOrDefault(c =>
            string.Equals(c.Nombre, modelo.NuevaCategoria.Trim(), StringComparison.OrdinalIgnoreCase));
        if (creada is not null) modelo.IdCategoria = creada.IdCategoria;

        modelo.NuevaCategoria = null;
        TempData["Exito"] = "Categoría creada y seleccionada.";
        return View(nameof(CrearProducto), modelo);
    }

    // Vuelve a la pantalla que abrió el alta llevándose el id del producto nuevo,
    // para que la venta o la compra en curso lo encuentren ya seleccionado.
    private IActionResult VolverAlOrigen(string? volverA, int idProducto)
    {
        if (string.IsNullOrWhiteSpace(volverA) || !Url.IsLocalUrl(volverA))
            return RedirectToAction(nameof(Productos));

        var separador = volverA.Contains('?') ? "&" : "?";
        return Redirect($"{volverA}{separador}idProductoNuevo={idProducto}");
    }

    private async Task CompletarListasAsync(ProductoFormViewModel modelo)
    {
        modelo.Categorias = await _categorias.ObtenerTodasAsync();
        modelo.Proveedores = await _proveedores.ObtenerTodosAsync();
    }

    [HttpGet]
    public async Task<IActionResult> EditarProducto(int id)
    {
        var producto = await _productos.ObtenerPorIdAsync(id);
        if (!producto.Ok || producto.Datos is null)
        {
            TempData["Error"] = producto.Error ?? "No se encontró el producto.";
            return RedirectToAction(nameof(Productos));
        }

        var p = producto.Datos;
        return View(new ProductoFormViewModel
        {
            IdProducto = p.IdProducto,
            Nombre = p.Nombre,
            Marca = p.Marca,
            IdCategoria = p.IdCategoria,
            PrecioVenta = p.PrecioVenta,
            DiasAvisoVencimiento = p.DiasAvisoVencimiento,
            StockDisponible = p.StockDisponible,
            Categorias = await _categorias.ObtenerTodasAsync()
        });

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarProducto(ProductoFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            await CompletarListasAsync(modelo);
            return View(modelo);
        }

        var resultado = await _productos.ActualizarAsync(modelo.IdProducto, new ProductoCreateDto(
            modelo.Nombre, modelo.Marca, modelo.IdCategoria,
            modelo.PrecioVenta, modelo.DiasAvisoVencimiento));

        if (!resultado.Ok)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            await CompletarListasAsync(modelo);
            return View(modelo);
        }

        TempData["Exito"] = "Producto actualizado.";
        return RedirectToAction(nameof(Productos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SoloAdministrador]
    public async Task<IActionResult> EliminarProducto(int id)
    {
        var resultado = await _productos.EliminarAsync(id);
        if (resultado.Ok) TempData["Exito"] = "Producto eliminado.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Productos));
    }

    // ---------- Listados de control ----------

    // Muestra únicamente los productos por debajo del stock mínimo
    [HttpGet]
    public async Task<IActionResult> Listado() =>
        View(await _productos.ObtenerStockCriticoAsync());

    [HttpGet]
    public async Task<IActionResult> ProximosAVencer()
    {
        var lotes = await _productos.ObtenerProximosAVencerAsync();
        return View(lotes);
    }

    // ---------- Lotes ----------

    [HttpGet]
    public async Task<IActionResult> Lotes(int id)
    {
        var producto = await _productos.ObtenerPorIdAsync(id);
        if (!producto.Ok || producto.Datos is null)
        {
            TempData["Error"] = producto.Error ?? "No se encontró el producto.";
            return RedirectToAction(nameof(Productos));
        }

        return View(new LotesProductoViewModel
        {
            IdProducto = id,
            NombreProducto = producto.Datos.Nombre,
            Lotes = await _lotes.ObtenerPorProductoAsync(id)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarLote(LotesProductoViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Revisá los datos del lote.";
            return RedirectToAction(nameof(Lotes), new { id = modelo.IdProducto });
        }

        var resultado = await _lotes.CrearAsync(modelo.IdProducto,
            new LoteCreateDto(modelo.NroLote, modelo.FechaVencimiento, modelo.Cantidad));

        if (resultado.Ok) TempData["Exito"] = "Lote registrado.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Lotes), new { id = modelo.IdProducto });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarLote(int idLote, int idProducto)
    {
        var resultado = await _lotes.EliminarAsync(idLote);
        if (resultado.Ok) TempData["Exito"] = "Lote eliminado.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Lotes), new { id = idProducto });
    }

    // ---------- Categorías ----------

    [HttpGet]
    public async Task<IActionResult> Categorias()
    {
        return View(new CategoriaFormViewModel
        {
            Categorias = await _categorias.ObtenerTodasAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCategoria(CategoriaFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            modelo.Categorias = await _categorias.ObtenerTodasAsync();
            return View(nameof(Categorias), modelo);
        }

        var resultado = await _categorias.CrearAsync(
            new CategoriaCreateDto(modelo.Nombre, modelo.Descripcion));

        if (resultado.Ok) TempData["Exito"] = "Categoría creada.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Categorias));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SoloAdministrador]
    public async Task<IActionResult> EliminarCategoria(int id)
    {
        var resultado = await _categorias.EliminarAsync(id);
        if (resultado.Ok) TempData["Exito"] = "Categoría eliminada.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Categorias));
    }

    // ---------- Proveedores del producto ----------

    [HttpGet]
    public async Task<IActionResult> ProveedoresDelProducto(int id)
    {
        var producto = await _productos.ObtenerPorIdAsync(id);
        if (!producto.Ok || producto.Datos is null)
        {
            TempData["Error"] = producto.Error ?? "No se encontró el producto.";
            return RedirectToAction(nameof(Productos));
        }

        return View(new ProductosAsociadosViewModel
        {
            IdProducto = id,
            NombreProducto = producto.Datos.Nombre,
            Asociaciones = await _proveedores.ObtenerProveedoresDeProductoAsync(id),
            Proveedores = await _proveedores.ObtenerTodosAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsociarProveedor(ProductosAsociadosViewModel modelo)
    {
        var resultado = await _proveedores.AsociarProductoAsync(modelo.IdProducto,
            new ProductoProveedorCreateDto(modelo.IdProveedor, modelo.PrecioCompra));

        if (resultado.Ok) TempData["Exito"] = "Proveedor asociado al producto.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(ProveedoresDelProducto), new { id = modelo.IdProducto });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPrecioProveedor(int idProducto, int idAsociacion, double precioCompra)
    {
        if (precioCompra <= 0)
        {
            TempData["Error"] = "El precio de compra debe ser mayor a 0.";
            return RedirectToAction(nameof(ProveedoresDelProducto), new { id = idProducto });
        }

        var resultado = await _proveedores.ActualizarPrecioAsync(idProducto, idAsociacion, precioCompra);
        if (resultado.Ok) TempData["Exito"] = "Precio actualizado.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(ProveedoresDelProducto), new { id = idProducto });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesasociarProveedor(int idProducto, int idAsociacion)
    {
        var resultado = await _proveedores.DesasociarProductoAsync(idProducto, idAsociacion);
        if (resultado.Ok) TempData["Exito"] = "Asociación eliminada.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(ProveedoresDelProducto), new { id = idProducto });
    }
}

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

    [HttpGet]
    public async Task<IActionResult> CrearProducto()
    {
        var modelo = new ProductoFormViewModel
        {
            Categorias = await _categorias.ObtenerTodasAsync()
        };
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearProducto(ProductoFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            modelo.Categorias = await _categorias.ObtenerTodasAsync();
            return View(modelo);
        }

        var resultado = await _productos.CrearAsync(new ProductoCreateDto(
            modelo.Nombre, modelo.Marca, modelo.IdCategoria,
            modelo.PrecioVenta, modelo.DiasAvisoVencimiento));

        if (!resultado.Ok)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            modelo.Categorias = await _categorias.ObtenerTodasAsync();
            return View(modelo);
        }

        TempData["Exito"] = "Producto agregado. Cargá una compra o un lote para darle stock.";
        return RedirectToAction(nameof(Productos));
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
            modelo.Categorias = await _categorias.ObtenerTodasAsync();
            return View(modelo);
        }

        var resultado = await _productos.ActualizarAsync(modelo.IdProducto, new ProductoCreateDto(
            modelo.Nombre, modelo.Marca, modelo.IdCategoria,
            modelo.PrecioVenta, modelo.DiasAvisoVencimiento));

        if (!resultado.Ok)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            modelo.Categorias = await _categorias.ObtenerTodasAsync();
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

    [HttpGet]
    public async Task<IActionResult> Listado()
    {
        var productos = await _productos.ObtenerTodosAsync();
        ViewBag.Criticos = await _productos.ObtenerStockCriticoAsync();
        return View(productos);
    }

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
    public async Task<IActionResult> DesasociarProveedor(int idProducto, int idAsociacion)
    {
        var resultado = await _proveedores.DesasociarProductoAsync(idProducto, idAsociacion);
        if (resultado.Ok) TempData["Exito"] = "Asociación eliminada.";
        else TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(ProveedoresDelProducto), new { id = idProducto });
    }
}

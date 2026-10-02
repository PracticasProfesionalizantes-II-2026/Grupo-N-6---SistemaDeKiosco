using System.ComponentModel.DataAnnotations;
using KioPlusFront.Models.Api;

namespace KioPlusFront.Models.ViewModels;

public class ProductoFormViewModel
{
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "Ingresá el nombre del producto")]
    [Display(Name = "Nombre del producto")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá la marca")]
    [Display(Name = "Marca")]
    public string Marca { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Elegí una categoría")]
    [Display(Name = "Categoría")]
    public int IdCategoria { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    [Display(Name = "Precio de venta")]
    public double PrecioVenta { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Los días de aviso no pueden ser negativos")]
    [Display(Name = "Días de aviso por vencimiento")]
    public int DiasAvisoVencimiento { get; set; }

    // Solo informativo al editar: el stock es la suma de los lotes del producto
    public int StockDisponible { get; set; }

    public IReadOnlyList<CategoriaDto> Categorias { get; set; } = Array.Empty<CategoriaDto>();
    public IReadOnlyList<ProveedorDto> Proveedores { get; set; } = Array.Empty<ProveedorDto>();

    // --- Primer lote, opcional: es la forma de que el producto nazca con stock ---

    [Display(Name = "Número de lote")]
    public string? NroLote { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de vencimiento")]
    public DateTime? FechaVencimiento { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa")]
    [Display(Name = "Cantidad inicial")]
    public int CantidadInicial { get; set; }

    // --- Primer proveedor, opcional ---

    [Display(Name = "Proveedor")]
    public int IdProveedor { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
    [Display(Name = "Precio de compra")]
    public double PrecioCompra { get; set; }

    // Alta rápida de categoría desde esta misma pantalla, para no perder lo cargado
    [Display(Name = "Nueva categoría")]
    public string? NuevaCategoria { get; set; }

    // Pantalla a la que hay que volver al terminar. La usa el botón "Nuevo Producto"
    // de una venta o de una compra en curso para no perder lo que se estaba armando.
    public string? VolverA { get; set; }
}

public class ListadoProductosViewModel
{
    public IReadOnlyList<ProductoDto> Productos { get; set; } = Array.Empty<ProductoDto>();
    public IReadOnlyList<CategoriaDto> Categorias { get; set; } = Array.Empty<CategoriaDto>();

    [Display(Name = "Buscar")]
    public string? Nombre { get; set; }

    [Display(Name = "Categoría")]
    public int? IdCategoria { get; set; }

    [Display(Name = "Marca")]
    public string? Marca { get; set; }

    public string NombreCategoria(int id) =>
        Categorias.FirstOrDefault(c => c.IdCategoria == id)?.Nombre ?? "-";
}

public class LotesProductoViewModel
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public IReadOnlyList<LoteDto> Lotes { get; set; } = Array.Empty<LoteDto>();

    [Required(ErrorMessage = "Ingresá el número de lote")]
    [Display(Name = "Número de lote")]
    public string NroLote { get; set; } = string.Empty;

    // Vacía significa que el producto no vence (encendedores, pilas, etc.)
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de vencimiento")]
    public DateTime? FechaVencimiento { get; set; } = DateTime.Today.AddMonths(1);

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    [Display(Name = "Cantidad")]
    public int Cantidad { get; set; } = 1;

    public int TotalDisponible => Lotes.Sum(l => l.Cantidad);
}

public class CategoriaFormViewModel
{
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "Ingresá el nombre de la categoría")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Descripción (Opcional)")]
    public string? Descripcion { get; set; }

    public IReadOnlyList<CategoriaDto> Categorias { get; set; } = Array.Empty<CategoriaDto>();
}

public class ProductosAsociadosViewModel
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public IReadOnlyList<ProductoProveedorDto> Asociaciones { get; set; } = Array.Empty<ProductoProveedorDto>();
    public IReadOnlyList<ProveedorDto> Proveedores { get; set; } = Array.Empty<ProveedorDto>();

    [Range(1, int.MaxValue, ErrorMessage = "Elegí un proveedor")]
    [Display(Name = "Proveedor")]
    public int IdProveedor { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    [Display(Name = "Precio de compra")]
    public double PrecioCompra { get; set; }

    public string NombreProveedor(int id) =>
        Proveedores.FirstOrDefault(p => p.IdProveedor == id)?.NombreRazonSocial ?? "-";

    // Un proveedor se asocia una sola vez a cada producto
    public IEnumerable<ProveedorDto> ProveedoresDisponibles =>
        Proveedores.Where(p => Asociaciones.All(a => a.IdProveedor != p.IdProveedor));
}

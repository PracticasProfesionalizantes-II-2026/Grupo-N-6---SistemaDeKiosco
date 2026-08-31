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

    // Solo informativo al editar: el stock se mueve por compras y ventas
    public int StockDisponible { get; set; }

    public IReadOnlyList<CategoriaDto> Categorias { get; set; } = Array.Empty<CategoriaDto>();
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

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de vencimiento")]
    public DateTime FechaVencimiento { get; set; } = DateTime.Today.AddMonths(1);

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    [Display(Name = "Cantidad")]
    public int Cantidad { get; set; } = 1;
}

public class CategoriaFormViewModel
{
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "Ingresá el nombre de la categoría")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá una descripción")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

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
}

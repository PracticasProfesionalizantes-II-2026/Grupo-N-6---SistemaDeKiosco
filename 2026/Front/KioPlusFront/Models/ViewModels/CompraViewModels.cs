using System.ComponentModel.DataAnnotations;
using KioPlusFront.Models.Api;

namespace KioPlusFront.Models.ViewModels;

// Renglón de la compra mientras se está armando (vive en la sesión).
public class ItemCompra
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
    public double Subtotal => Cantidad * PrecioUnitario;
}

public class NuevaCompraViewModel
{
    public List<ItemCompra> Items { get; set; } = new();
    public IReadOnlyList<ProductoDto> Productos { get; set; } = Array.Empty<ProductoDto>();
    public IReadOnlyList<ProveedorDto> Proveedores { get; set; } = Array.Empty<ProveedorDto>();

    [Range(1, int.MaxValue, ErrorMessage = "Elegí un proveedor")]
    [Display(Name = "Proveedor")]
    public int IdProveedor { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de la compra")]
    public DateTime FechaHora { get; set; } = DateTime.Today;

    public int? IdProductoSeleccionado { get; set; }
    public int Cantidad { get; set; } = 1;
    public double PrecioUnitario { get; set; }

    public double Total => Items.Sum(i => i.Subtotal);
}

public class ListadoComprasViewModel
{
    public IReadOnlyList<CompraListadoDto> Compras { get; set; } = Array.Empty<CompraListadoDto>();
    public IReadOnlyList<ProveedorDto> Proveedores { get; set; } = Array.Empty<ProveedorDto>();

    [DataType(DataType.Date)]
    [Display(Name = "Desde")]
    public DateTime? FechaDesde { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Hasta")]
    public DateTime? FechaHasta { get; set; }

    [Display(Name = "Proveedor")]
    public int? IdProveedor { get; set; }

    public double TotalListado => Compras.Sum(c => c.MontoTotal);
}

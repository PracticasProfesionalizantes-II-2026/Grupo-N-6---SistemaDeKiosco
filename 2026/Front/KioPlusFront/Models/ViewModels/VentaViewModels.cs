using System.ComponentModel.DataAnnotations;
using KioPlusFront.Models.Api;

namespace KioPlusFront.Models.ViewModels;

// Renglón del carrito mientras la venta se está armando (vive en la sesión,
// todavía no existe en la base).
public class ItemCarrito
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
    public double Subtotal => Cantidad * PrecioUnitario;
}

public class NuevaVentaViewModel
{
    public List<ItemCarrito> Items { get; set; } = new();
    public IReadOnlyList<ProductoDto> Productos { get; set; } = Array.Empty<ProductoDto>();
    public IReadOnlyList<CuentaCorrienteClienteDto> Clientes { get; set; } = Array.Empty<CuentaCorrienteClienteDto>();

    public int? IdProductoSeleccionado { get; set; }
    public int Cantidad { get; set; } = 1;

    public FormaDePago FormaPago { get; set; } = FormaDePago.PagadoAlMomento;
    public int IdCliente { get; set; } = 1;

    public double Total => Items.Sum(i => i.Subtotal);
}

public class ListadoVentasViewModel
{
    public IReadOnlyList<VentaListadoDto> Ventas { get; set; } = Array.Empty<VentaListadoDto>();

    [DataType(DataType.Date)]
    [Display(Name = "Desde")]
    public DateTime? FechaDesde { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Hasta")]
    public DateTime? FechaHasta { get; set; }

    [Display(Name = "Vendedor")]
    public int? IdUsuario { get; set; }

    [Display(Name = "Cliente")]
    public int? IdCliente { get; set; }

    [Display(Name = "Importe mayor a")]
    public double? ImporteMayorA { get; set; }

    [Display(Name = "Importe menor a")]
    public double? ImporteMenorA { get; set; }

    public IReadOnlyList<UsuarioDto> Vendedores { get; set; } = Array.Empty<UsuarioDto>();
    public IReadOnlyList<CuentaCorrienteClienteDto> Clientes { get; set; } = Array.Empty<CuentaCorrienteClienteDto>();

    public double TotalListado => Ventas.Sum(v => v.MontoTotal);
}

public class MasVendidosViewModel
{
    [DataType(DataType.Date)]
    [Display(Name = "Desde")]
    public DateTime FechaDesde { get; set; } = DateTime.Today.AddMonths(-1);

    [DataType(DataType.Date)]
    [Display(Name = "Hasta")]
    public DateTime FechaHasta { get; set; } = DateTime.Today;

    [Display(Name = "Criterio")]
    public string Criterio { get; set; } = "cantidad";

    [Display(Name = "Cantidad de productos")]
    [Range(1, 50, ErrorMessage = "El límite debe estar entre 1 y 50")]
    public int Limite { get; set; } = 5;

    public IReadOnlyList<MasVendidoCantidadDto> PorCantidad { get; set; } = Array.Empty<MasVendidoCantidadDto>();
    public IReadOnlyList<MasVendidoMontoDto> PorMonto { get; set; } = Array.Empty<MasVendidoMontoDto>();
}

namespace KioPlusFront.Models.Api;

public enum FormaDePago
{
    CuentaCorriente,
    PagadoAlMomento
}

public enum EstadoVenta
{
    Pagado,
    NoPagado
}

public record VentaDto(
    int IdVenta,
    int IdUsuario,
    DateTime FechaHora,
    double MontoTotal,
    int IdCuentaCorrienteCliente,
    FormaDePago FormaPago,
    DateTime? FechaPago,
    EstadoVenta Estado,
    bool Finalizada,
    double MontoPagado);

public record VentaCreateDto(
    DateTime FechaHora,
    int IdUsuario,
    int IdCuentaCorrienteCliente,
    FormaDePago FormaPago,
    // Vacía mientras la venta no se haya cobrado
    DateTime? FechaPago);

public record VentaListadoDto(
    int IdVenta,
    DateTime FechaHora,
    int IdUsuario,
    string Vendedor,
    int IdCuentaCorrienteCliente,
    string Cliente,
    double MontoTotal,
    FormaDePago FormaPago,
    EstadoVenta Estado);

public record DetalleVentaListadoDto(
    int IdDetalle,
    int IdProducto,
    string Producto,
    int Cantidad,
    double PrecioUnitario,
    double Subtotal);

public record DetalleVentaCreateDto(int IdProducto, int Cantidad);

public record DetalleVentaUpdateDto(int Cantidad);

// Cobro de una venta adeudada. Sin fecha se toma la de hoy.
public record PagoVentaDto(DateTime? FechaPago);

// Venta en cuenta corriente todavía impaga, para la pantalla de deuda del cliente
public record VentaAdeudadaDto(
    int IdVenta,
    DateTime FechaHora,
    string Vendedor,
    double MontoTotal,
    double MontoPagado,
    double SaldoPendiente);

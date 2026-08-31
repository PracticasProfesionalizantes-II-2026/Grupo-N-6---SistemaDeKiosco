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
    DateTime FechaPago,
    EstadoVenta Estado);

public record VentaCreateDto(
    DateTime FechaHora,
    int IdUsuario,
    int IdCuentaCorrienteCliente,
    FormaDePago FormaPago,
    DateTime FechaPago);

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

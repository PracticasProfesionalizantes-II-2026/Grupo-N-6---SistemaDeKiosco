namespace KioPlusFront.Models.Api;

public record CompraDto(int IdCompraProveedor, DateTime FechaHora, int IdProveedor, double MontoTotal);

public record CompraCreateDto(DateTime FechaHora, int IdProveedor);

public record CompraListadoDto(
    int IdCompraProveedor,
    DateTime FechaHora,
    int IdProveedor,
    string Proveedor,
    double MontoTotal);

public record DetalleCompraListadoDto(
    int IdDetalleCompra,
    int IdProducto,
    string Producto,
    int Cantidad,
    double PrecioUnitario,
    double Subtotal);

public record DetalleCompraCreateDto(int IdProducto, int Cantidad, double PrecioUnitario);

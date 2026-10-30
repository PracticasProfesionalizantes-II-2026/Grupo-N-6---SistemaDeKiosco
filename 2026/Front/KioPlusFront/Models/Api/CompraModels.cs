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
    double Subtotal,
    string? NroLote,
    DateTime? FechaVencimiento);

// Borrador de compra que la IA leyó de una factura en PDF
public record FacturaInterpretadaDto(
    int? IdProveedor,
    string? Proveedor,
    DateTime? Fecha,
    IReadOnlyList<RenglonFacturaDto> Renglones);

// IdProducto en null: el renglón no coincide con ningún producto del sistema
public record RenglonFacturaDto(
    int? IdProducto,
    string Descripcion,
    int Cantidad,
    double PrecioUnitario);

// Cada renglón de compra ingresa mercadería, así que genera un lote
public record DetalleCompraCreateDto(
    int IdProducto,
    int Cantidad,
    double PrecioUnitario,
    string? NroLote,
    DateTime? FechaVencimiento);

namespace Clases_KioPlus.Logica.DTOs;

public record DetalleVentaDto(
    int IdDetalle,
    int IdVenta,
    int IdProducto,
    int Cantidad,
    double PrecioUnitario,
    double Subtotal);

public record DetalleVentaCreateDto(int IdProducto, int Cantidad);

public record DetalleVentaUpdateDto(int Cantidad);

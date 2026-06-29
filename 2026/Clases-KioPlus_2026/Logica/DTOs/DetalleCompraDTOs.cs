namespace Clases_KioPlus.Logica.DTOs;

public record DetalleCompraDto(
    int IdDetalleCompra,
    int IdCompra,
    int IdProducto,
    int Cantidad,
    double PrecioUnitario,
    double Subtotal);

public record DetalleCompraCreateDto(int IdProducto, int Cantidad, double PrecioUnitario);

public record DetalleCompraUpdateDto(int IdCompra, int IdProducto, int Cantidad, double PrecioUnitario);

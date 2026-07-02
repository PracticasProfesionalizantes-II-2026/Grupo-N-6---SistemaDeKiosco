using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record DetalleCompraDto(
    int IdDetalleCompra,
    int IdCompra,
    int IdProducto,
    int Cantidad,
    double PrecioUnitario,
    double Subtotal);

public record DetalleCompraCreateDto(
    [property: Range(1, int.MaxValue, ErrorMessage = "idProducto inválido")] int IdProducto,
    [property: Range(1, int.MaxValue, ErrorMessage = "cantidad debe ser mayor a 0")] int Cantidad,
    [property: Range(0.01, double.MaxValue, ErrorMessage = "precioUnitario debe ser mayor a 0")] double PrecioUnitario);

public record DetalleCompraUpdateDto(
    [property: Range(1, int.MaxValue, ErrorMessage = "idCompra inválido")] int IdCompra,
    [property: Range(1, int.MaxValue, ErrorMessage = "idProducto inválido")] int IdProducto,
    [property: Range(1, int.MaxValue, ErrorMessage = "cantidad debe ser mayor a 0")] int Cantidad,
    [property: Range(0.01, double.MaxValue, ErrorMessage = "precioUnitario debe ser mayor a 0")] double PrecioUnitario);

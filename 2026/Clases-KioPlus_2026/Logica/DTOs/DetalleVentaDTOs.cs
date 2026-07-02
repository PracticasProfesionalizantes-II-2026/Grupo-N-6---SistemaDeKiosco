using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record DetalleVentaDto(
    int IdDetalle,
    int IdVenta,
    int IdProducto,
    int Cantidad,
    double PrecioUnitario,
    double Subtotal);

public record DetalleVentaCreateDto(
    [property: Range(1, int.MaxValue, ErrorMessage = "idProducto inválido")] int IdProducto,
    [property: Range(1, int.MaxValue, ErrorMessage = "cantidad debe ser mayor a 0")] int Cantidad);

public record DetalleVentaUpdateDto(
    [property: Range(1, int.MaxValue, ErrorMessage = "cantidad debe ser mayor a 0")] int Cantidad);

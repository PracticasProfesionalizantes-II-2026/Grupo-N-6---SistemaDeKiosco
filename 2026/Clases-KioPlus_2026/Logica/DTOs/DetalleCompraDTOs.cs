using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record DetalleCompraDto(
    int IdDetalleCompra,
    int IdCompra,
    int IdProducto,
    int Cantidad,
    double PrecioUnitario,
    double Subtotal,
    int? IdLote);

// Cada renglón de compra ingresa mercadería, así que genera un lote.
// nroLote y fechaVencimiento son opcionales: sin número se numera solo a partir
// de la compra, y sin fecha el lote se toma como "no vence".
public record DetalleCompraCreateDto(
    [property: Range(1, int.MaxValue, ErrorMessage = "Elegí un producto")] int IdProducto,
    [property: Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")] int Cantidad,
    [property: Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0")] double PrecioUnitario,
    string? NroLote,
    DateTime? FechaVencimiento);

public record DetalleCompraUpdateDto(
    [property: Range(1, int.MaxValue, ErrorMessage = "Elegí una compra")] int IdCompra,
    [property: Range(1, int.MaxValue, ErrorMessage = "Elegí un producto")] int IdProducto,
    [property: Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")] int Cantidad,
    [property: Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0")] double PrecioUnitario,
    string? NroLote,
    DateTime? FechaVencimiento);

// Renglón del detalle de una compra con el nombre del producto y su lote resueltos
public record DetalleCompraListadoDto(
    int IdDetalleCompra,
    int IdProducto,
    string Producto,
    int Cantidad,
    double PrecioUnitario,
    double Subtotal,
    string? NroLote,
    DateTime? FechaVencimiento);

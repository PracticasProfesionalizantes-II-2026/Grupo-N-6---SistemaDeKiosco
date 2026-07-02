using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record ProductoProveedorDto(int IdProductoProveedor, int IdProveedor, double PrecioCompra);

public record ProductoProveedorCreateDto(
    [property: Range(1, int.MaxValue, ErrorMessage = "idProveedor inválido")] int IdProveedor,
    [property: Range(0.01, double.MaxValue, ErrorMessage = "precioCompra debe ser mayor a 0")] double PrecioCompra);

public record ProductoProveedorUpdateDto(
    [property: Range(0.01, double.MaxValue, ErrorMessage = "precioCompra debe ser mayor a 0")] double PrecioCompra);

using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record ProductoProveedorDto(int IdProductoProveedor, int IdProveedor, double PrecioCompra);

// La misma asociación leída desde el proveedor: qué productos le compra el kiosco
public record ProductoDelProveedorDto(
    int IdProductoProveedor,
    int IdProducto,
    string Producto,
    string Marca,
    double PrecioCompra);

public record ProductoProveedorCreateDto(
    [property: Range(1, int.MaxValue, ErrorMessage = "Elegí un proveedor")] int IdProveedor,
    [property: Range(0.01, double.MaxValue, ErrorMessage = "El precio de compra debe ser mayor a 0")] double PrecioCompra);

public record ProductoProveedorUpdateDto(
    [property: Range(0.01, double.MaxValue, ErrorMessage = "El precio de compra debe ser mayor a 0")] double PrecioCompra);

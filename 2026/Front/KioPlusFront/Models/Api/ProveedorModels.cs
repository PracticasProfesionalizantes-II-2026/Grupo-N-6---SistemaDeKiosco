namespace KioPlusFront.Models.Api;

public record ProveedorDto(
    int IdProveedor,
    string NombreRazonSocial,
    string Telefono,
    string Direccion,
    string? CorreoElectronico,
    string? Observaciones);

public record ProveedorCreateDto(
    string NombreRazonSocial,
    string Telefono,
    string Direccion,
    string? CorreoElectronico,
    string? Observaciones);

public record ProductoProveedorDto(int IdProductoProveedor, int IdProveedor, double PrecioCompra);

// La misma asociación leída desde el proveedor
public record ProductoDelProveedorDto(
    int IdProductoProveedor,
    int IdProducto,
    string Producto,
    string Marca,
    double PrecioCompra);

public record ProductoProveedorCreateDto(int IdProveedor, double PrecioCompra);

public record ProductoProveedorUpdateDto(double PrecioCompra);

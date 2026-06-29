namespace Clases_KioPlus.Logica.DTOs;

public record ProductoProveedorDto(int IdProductoProveedor, int IdProveedor, double PrecioCompra);

public record ProductoProveedorCreateDto(int IdProveedor, double PrecioCompra);

public record ProductoProveedorUpdateDto(double PrecioCompra);

using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record ProductoDto(
    int IdProducto,
    string Nombre,
    string Marca,
    int IdCategoria,
    double PrecioVenta,
    int DiasAvisoVencimiento,
    int StockDisponible);

public record ProductoCreateDto(
    [property: Required] string Nombre,
    [property: Required] string Marca,
    [property: Range(1, int.MaxValue, ErrorMessage = "idCategoria inválido")] int IdCategoria,
    [property: Range(0.01, double.MaxValue, ErrorMessage = "precioVenta debe ser mayor a 0")] double PrecioVenta,
    [property: Range(0, int.MaxValue, ErrorMessage = "diasAvisoVencimiento no puede ser negativo")] int DiasAvisoVencimiento);

// Resumen de lote usado dentro del detalle de un producto
public record LoteResumenDto(int IdLote, DateTime FechaVencimiento, int Cantidad);

public record ProductoDetalleDto(
    int IdProducto,
    string Nombre,
    string Marca,
    int IdCategoria,
    double PrecioVenta,
    int DiasAvisoVencimiento,
    int StockDisponible,
    IEnumerable<LoteResumenDto> Lotes);

public record StockCriticoDto(string Nombre, int StockDisponible);

public record ProximoVencimientoDto(string Nombre, string NroLote, int Cantidad, int DiasParaQueVenza);

public record MasVendidoCantidadDto(string Nombre, int Cantidad);

public record MasVendidoMontoDto(string Nombre, double MontoGenerado);

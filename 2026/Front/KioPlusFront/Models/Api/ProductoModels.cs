namespace KioPlusFront.Models.Api;

public record ProductoDto(
    int IdProducto,
    string Nombre,
    string Marca,
    int IdCategoria,
    double PrecioVenta,
    int DiasAvisoVencimiento,
    int StockDisponible);

public record ProductoCreateDto(
    string Nombre,
    string Marca,
    int IdCategoria,
    double PrecioVenta,
    int DiasAvisoVencimiento);

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

public record CategoriaDto(int IdCategoria, string Nombre, string Descripcion);

public record CategoriaCreateDto(string Nombre, string Descripcion);

public record LoteDto(int IdLote, int IdProducto, string NroLote, DateTime FechaVencimiento, int Cantidad);

public record LoteCreateDto(string NroLote, DateTime FechaVencimiento, int Cantidad);

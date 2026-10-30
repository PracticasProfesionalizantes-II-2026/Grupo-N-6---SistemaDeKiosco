namespace Clases_KioPlus.Logica.DTOs;

// Lo que la IA pudo leer de una factura de proveedor. No registra nada: es un
// borrador para precargar la pantalla de nueva compra, que el usuario confirma.
public record FacturaInterpretadaDto(
    int? IdProveedor,
    string? Proveedor,
    DateTime? Fecha,
    IEnumerable<RenglonFacturaDto> Renglones);

// IdProducto queda en null cuando el renglón no coincide con ningún producto del sistema.
public record RenglonFacturaDto(
    int? IdProducto,
    string Descripcion,
    int Cantidad,
    double PrecioUnitario);

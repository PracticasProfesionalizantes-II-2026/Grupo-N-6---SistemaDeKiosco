using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record LoteDto(
    int IdLote,
    int IdProducto,
    string NroLote,
    DateTime? FechaVencimiento,
    int CantidadInicial,
    int Cantidad);

// fechaVencimiento es opcional: sin fecha el lote se toma como "no vence"
// y queda último en el orden de consumo.
public record LoteCreateDto(
    [property: Required(ErrorMessage = "Ingresá el número de lote")] string NroLote,
    DateTime? FechaVencimiento,
    [property: Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")] int Cantidad);

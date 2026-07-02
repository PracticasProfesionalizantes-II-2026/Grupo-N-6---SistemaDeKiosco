using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record LoteDto(int IdLote, int IdProducto, string NroLote, DateTime FechaVencimiento, int Cantidad);

public record LoteCreateDto(
    [property: Required] string NroLote,
    DateTime FechaVencimiento,
    [property: Range(1, int.MaxValue, ErrorMessage = "cantidad debe ser mayor a 0")] int Cantidad);

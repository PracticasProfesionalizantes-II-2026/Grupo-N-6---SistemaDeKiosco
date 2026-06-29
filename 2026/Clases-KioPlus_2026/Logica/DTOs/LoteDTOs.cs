namespace Clases_KioPlus.Logica.DTOs;

public record LoteDto(int IdLote, int IdProducto, string NroLote, DateTime FechaVencimiento, int Cantidad);

public record LoteCreateDto(string NroLote, DateTime FechaVencimiento, int Cantidad);

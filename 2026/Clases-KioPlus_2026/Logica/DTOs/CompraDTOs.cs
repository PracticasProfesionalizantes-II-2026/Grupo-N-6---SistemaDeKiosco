namespace Clases_KioPlus.Logica.DTOs;

public record CompraDto(int IdCompraProveedor, DateTime FechaHora, int IdProveedor, double MontoTotal);

public record CompraCreateDto(DateTime FechaHora, int IdProveedor);

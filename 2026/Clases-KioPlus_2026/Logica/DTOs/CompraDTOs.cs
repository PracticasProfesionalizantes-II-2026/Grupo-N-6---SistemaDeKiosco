using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record CompraDto(int IdCompraProveedor, DateTime FechaHora, int IdProveedor, double MontoTotal);

public record CompraCreateDto(
    DateTime FechaHora,
    [property: Range(1, int.MaxValue, ErrorMessage = "idProveedor inválido")] int IdProveedor);

// Forma enriquecida usada por el listado de compras: incluye el nombre del proveedor.
public record CompraListadoDto(
    int IdCompraProveedor,
    DateTime FechaHora,
    int IdProveedor,
    string Proveedor,
    double MontoTotal);

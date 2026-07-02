using System.ComponentModel.DataAnnotations;
using Clases_KioPlus.Models;

namespace Clases_KioPlus.Logica.DTOs;

public record VentaDto(
    int IdVenta,
    int IdUsuario,
    DateTime FechaHora,
    double MontoTotal,
    int IdCuentaCorrienteCliente,
    Venta.FormaDePago FormaPago,
    DateTime FechaPago,
    Venta.EstadoVenta Estado);

public record VentaCreateDto(
    DateTime FechaHora,
    [property: Range(1, int.MaxValue, ErrorMessage = "idUsuario inválido")] int IdUsuario,
    [property: Range(1, int.MaxValue, ErrorMessage = "idCuentaCorrienteCliente inválido")] int IdCuentaCorrienteCliente,
    Venta.FormaDePago FormaPago,
    DateTime FechaPago);

// Forma reducida usada por los filtros de ventas
public record VentaFiltroDto(DateTime FechaHora, int Vendedor, int Cliente, double Importe);

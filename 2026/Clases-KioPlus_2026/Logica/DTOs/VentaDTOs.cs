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
    // 0 o vacío se interpreta como Consumidor Final (id 1)
    [property: Range(0, int.MaxValue, ErrorMessage = "idCuentaCorrienteCliente inválido")] int IdCuentaCorrienteCliente,
    Venta.FormaDePago FormaPago,
    DateTime FechaPago);

// Forma enriquecida usada por el listado de ventas: incluye los nombres de
// vendedor y cliente para que el front no tenga que resolver cada id por separado.
public record VentaListadoDto(
    int IdVenta,
    DateTime FechaHora,
    int IdUsuario,
    string Vendedor,
    int IdCuentaCorrienteCliente,
    string Cliente,
    double MontoTotal,
    Venta.FormaDePago FormaPago,
    Venta.EstadoVenta Estado);

// Renglón del detalle de una venta con el nombre del producto resuelto
public record DetalleVentaListadoDto(
    int IdDetalle,
    int IdProducto,
    string Producto,
    int Cantidad,
    double PrecioUnitario,
    double Subtotal);

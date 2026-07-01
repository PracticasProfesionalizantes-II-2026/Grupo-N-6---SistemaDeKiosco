using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface IDetalleVentaRepositorio
{
    Task<bool> VentaExiste(int idVenta);
    Task<Producto?> ObtenerProducto(int idProducto);
    Task<IEnumerable<DetalleVenta>> ObtenerPorVenta(int idVenta);
    Task<DetalleVenta?> ObtenerPorId(int id);
    Task<DetalleVenta> Agregar(DetalleVenta detalle);
    Task Actualizar(DetalleVenta detalle);
    Task Eliminar(DetalleVenta detalle);
    Task RecalcularMontoVenta(int idVenta);
}
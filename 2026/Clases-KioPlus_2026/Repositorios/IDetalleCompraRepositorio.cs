using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface IDetalleCompraRepositorio
{
    Task<bool> CompraExiste(int idCompra);
    Task<IEnumerable<DetalleCompra>> ObtenerPorCompra(int idCompra);
    Task<DetalleCompra?> ObtenerPorId(int id);
    Task<DetalleCompra> Agregar(DetalleCompra detalle);
    Task Actualizar(DetalleCompra detalle);
    Task Eliminar(DetalleCompra detalle);
    Task RecalcularMontoCompra(int idCompra);
}
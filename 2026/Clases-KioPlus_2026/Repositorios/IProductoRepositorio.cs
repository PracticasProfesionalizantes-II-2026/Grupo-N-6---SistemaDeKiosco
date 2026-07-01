using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface IProductoRepositorio
{
    Task<IEnumerable<Producto>> ObtenerTodos(string? nombre, int? idCategoria, string? marca);
    Task<Producto?> ObtenerPorId(int id);
    Task<Producto?> ObtenerConLotes(int id);
    Task<Producto> Agregar(Producto producto);
    Task Actualizar(Producto producto);
    Task Eliminar(Producto producto);
    Task<IEnumerable<Producto>> ObtenerConStockCritico(int umbral);
    Task<IEnumerable<Lote>> ObtenerLotesConProducto();
    Task<IEnumerable<(string Nombre, int Cantidad, double Monto)>> MasVendidos(DateTime desde, DateTime hasta);
}
using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface IProductoLogica
{
    Task<IEnumerable<ProductoDto>> ObtenerTodos(string? nombre, int? idCategoria, string? marca);
    Task<ProductoDetalleDto?> ObtenerPorId(int id);
    Task<int> Crear(ProductoCreateDto dto);
    Task<bool> Actualizar(int id, ProductoCreateDto dto);
    Task<bool> Eliminar(int id);
    Task<IEnumerable<StockCriticoDto>> ObtenerStockCritico();
    Task<IEnumerable<ProximoVencimientoDto>> ObtenerProximosAVencer();
    Task<IEnumerable<object>> MasVendidos(DateTime desde, DateTime hasta, string criterio, int limite);
}
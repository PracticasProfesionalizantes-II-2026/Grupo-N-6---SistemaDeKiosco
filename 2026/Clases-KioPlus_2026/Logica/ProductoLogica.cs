using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class ProductoLogica : IProductoLogica
{
    private const int UmbralStockCritico = 5;
    private readonly IProductoRepositorio _repo;
    public ProductoLogica(IProductoRepositorio repo) => _repo = repo;

    private static ProductoDto AMapa(Producto p) =>
        new(p.Id, p.Nombre, p.Marca, p.CategoriaId, p.PrecioVenta, p.DiasAvisoVencimiento, p.StockDisponible);

    public async Task<IEnumerable<ProductoDto>> ObtenerTodos(string? nombre, int? idCategoria, string? marca)
    {
        var productos = await _repo.ObtenerTodos(nombre, idCategoria, marca);
        return productos.Select(AMapa);
    }

    public async Task<ProductoDetalleDto?> ObtenerPorId(int id)
    {
        var p = await _repo.ObtenerConLotes(id);
        if (p is null) return null;

        var lotes = (p.Lotes ?? new List<Lote>())
            .Select(l => new LoteResumenDto(l.Id, l.FechaVencimiento, l.Cantidad));

        return new ProductoDetalleDto(
            p.Id, p.Nombre, p.Marca, p.CategoriaId, p.PrecioVenta,
            p.DiasAvisoVencimiento, p.StockDisponible, lotes);
    }

    public async Task<int> Crear(ProductoCreateDto dto)
    {
        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Marca = dto.Marca,
            CategoriaId = dto.IdCategoria,
            PrecioVenta = dto.PrecioVenta,
            DiasAvisoVencimiento = dto.DiasAvisoVencimiento,
            StockDisponible = 0
        };
        await _repo.Agregar(producto);
        return producto.Id;
    }

    public async Task<bool> Actualizar(int id, ProductoCreateDto dto)
    {
        var producto = await _repo.ObtenerPorId(id);
        if (producto is null) return false;

        producto.Nombre = dto.Nombre;
        producto.Marca = dto.Marca;
        producto.CategoriaId = dto.IdCategoria;
        producto.PrecioVenta = dto.PrecioVenta;
        producto.DiasAvisoVencimiento = dto.DiasAvisoVencimiento;
        await _repo.Actualizar(producto);
        return true;
    }

    public async Task<bool> Eliminar(int id)
    {
        var producto = await _repo.ObtenerPorId(id);
        if (producto is null) return false;

        await _repo.Eliminar(producto);
        return true;
    }

    public async Task<IEnumerable<StockCriticoDto>> ObtenerStockCritico()
    {
        var productos = await _repo.ObtenerConStockCritico(UmbralStockCritico);
        return productos.Select(p => new StockCriticoDto(p.Nombre, p.StockDisponible));
    }

    public async Task<IEnumerable<ProximoVencimientoDto>> ObtenerProximosAVencer()
    {
        var hoy = DateTime.Now.Date;
        var lotes = await _repo.ObtenerLotesConProducto();

        return lotes
            .Where(l => l.Producto is not null)
            .Select(l => new
            {
                Lote = l,
                Dias = (l.FechaVencimiento.Date - hoy).Days
            })
            .Where(x => x.Dias >= 0 && x.Dias <= x.Lote.Producto.DiasAvisoVencimiento)
            .Select(x => new ProximoVencimientoDto(
                x.Lote.Producto.Nombre, x.Lote.NroLote, x.Lote.Cantidad, x.Dias))
            .ToList();
    }

    public async Task<IEnumerable<object>> MasVendidos(DateTime desde, DateTime hasta, string criterio, int limite)
    {
        var datos = await _repo.MasVendidos(desde, hasta);

        if (string.Equals(criterio, "monto", StringComparison.OrdinalIgnoreCase))
        {
            return datos
                .OrderByDescending(d => d.Monto)
                .Take(limite)
                .Select(d => new MasVendidoMontoDto(d.Nombre, d.Monto))
                .ToList();
        }

        // por defecto: criterio = cantidad
        return datos
            .OrderByDescending(d => d.Cantidad)
            .Take(limite)
            .Select(d => new MasVendidoCantidadDto(d.Nombre, d.Cantidad))
            .ToList();
    }
}

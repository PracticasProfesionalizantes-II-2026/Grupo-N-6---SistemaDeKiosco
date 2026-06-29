using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;

public interface IDetalleVentaLogica
{
    Task<IEnumerable<DetalleVentaDto>> ObtenerPorVenta(int idVenta);
    Task<DetalleVentaDto?> ObtenerPorId(int id);
    Task<int?> Crear(int idVenta, DetalleVentaCreateDto dto);
    Task<bool> Actualizar(int idVenta, int id, DetalleVentaUpdateDto dto);
    Task<bool> Eliminar(int idVenta, int id);
}

public class DetalleVentaLogica : IDetalleVentaLogica
{
    private readonly IDetalleVentaRepositorio _repo;
    public DetalleVentaLogica(IDetalleVentaRepositorio repo) => _repo = repo;

    private static DetalleVentaDto AMapa(DetalleVenta d) =>
        new(d.Id, d.VentaId, d.ProductoId, d.Cantidad, d.PrecioUnitario, d.Subtotal);

    public async Task<IEnumerable<DetalleVentaDto>> ObtenerPorVenta(int idVenta)
    {
        var detalles = await _repo.ObtenerPorVenta(idVenta);
        return detalles.Select(AMapa);
    }

    public async Task<DetalleVentaDto?> ObtenerPorId(int id)
    {
        var d = await _repo.ObtenerPorId(id);
        return d is null ? null : AMapa(d);
    }

    // Devuelve null si la venta o el producto no existen
    public async Task<int?> Crear(int idVenta, DetalleVentaCreateDto dto)
    {
        if (!await _repo.VentaExiste(idVenta)) return null;

        var producto = await _repo.ObtenerProducto(dto.IdProducto);
        if (producto is null) return null;

        var detalle = new DetalleVenta
        {
            VentaId = idVenta,
            ProductoId = dto.IdProducto,
            Cantidad = dto.Cantidad,
            PrecioUnitario = producto.PrecioVenta,
            Subtotal = dto.Cantidad * producto.PrecioVenta
        };
        await _repo.Agregar(detalle);
        await _repo.RecalcularMontoVenta(idVenta);
        return detalle.Id;
    }

    public async Task<bool> Actualizar(int idVenta, int id, DetalleVentaUpdateDto dto)
    {
        var detalle = await _repo.ObtenerPorId(id);
        if (detalle is null || detalle.VentaId != idVenta) return false;

        detalle.Cantidad = dto.Cantidad;
        detalle.Subtotal = dto.Cantidad * detalle.PrecioUnitario;
        await _repo.Actualizar(detalle);
        await _repo.RecalcularMontoVenta(idVenta);
        return true;
    }

    public async Task<bool> Eliminar(int idVenta, int id)
    {
        var detalle = await _repo.ObtenerPorId(id);
        if (detalle is null || detalle.VentaId != idVenta) return false;

        await _repo.Eliminar(detalle);
        await _repo.RecalcularMontoVenta(idVenta);
        return true;
    }
}

using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class DetalleVentaLogica : IDetalleVentaLogica
{
    private readonly IDetalleVentaRepositorio _repo;
    public DetalleVentaLogica(IDetalleVentaRepositorio repo) => _repo = repo;

    private static DetalleVentaDto AMapa(DetalleVenta d) =>
        new(d.Id, d.VentaId, d.ProductoId, d.Cantidad, d.PrecioUnitario, d.Subtotal);

    public async Task<IEnumerable<DetalleVentaListadoDto>> ObtenerPorVenta(int idVenta)
    {
        var filas = await _repo.ObtenerPorVentaConProducto(idVenta);
        return filas.Select(f => new DetalleVentaListadoDto(
            f.Detalle.Id, f.Detalle.ProductoId, f.Producto,
            f.Detalle.Cantidad, f.Detalle.PrecioUnitario, f.Detalle.Subtotal));
    }

    public async Task<DetalleVentaDto?> ObtenerPorId(int id)
    {
        var d = await _repo.ObtenerPorId(id);
        return d is null ? null : AMapa(d);
    }

    // Registra el renglón y descuenta el stock del producto vendido.
    public async Task<ResultadoOperacion> Crear(int idVenta, DetalleVentaCreateDto dto)
    {
        if (!await _repo.VentaExiste(idVenta))
            return ResultadoOperacion.NoEncontrado("venta no encontrada");

        var producto = await _repo.ObtenerProducto(dto.IdProducto);
        if (producto is null)
            return ResultadoOperacion.NoEncontrado("producto no encontrado");

        if (producto.StockDisponible < dto.Cantidad)
            return ResultadoOperacion.Invalido(
                $"stock insuficiente para {producto.Nombre}: disponible {producto.StockDisponible}");

        var detalle = new DetalleVenta
        {
            VentaId = idVenta,
            ProductoId = dto.IdProducto,
            Cantidad = dto.Cantidad,
            PrecioUnitario = producto.PrecioVenta,
            Subtotal = dto.Cantidad * producto.PrecioVenta
        };
        await _repo.Agregar(detalle);
        await _repo.AjustarStock(dto.IdProducto, -dto.Cantidad);
        await _repo.RecalcularMontoVenta(idVenta);
        return ResultadoOperacion.Exito(detalle.Id);
    }

    // Al cambiar la cantidad solo se mueve la diferencia contra el stock.
    public async Task<ResultadoOperacion> Actualizar(int idVenta, int id, DetalleVentaUpdateDto dto)
    {
        var detalle = await _repo.ObtenerPorId(id);
        if (detalle is null || detalle.VentaId != idVenta)
            return ResultadoOperacion.NoEncontrado("detalle no encontrado");

        var diferencia = dto.Cantidad - detalle.Cantidad;

        if (diferencia > 0)
        {
            var producto = await _repo.ObtenerProducto(detalle.ProductoId);
            if (producto is null)
                return ResultadoOperacion.NoEncontrado("producto no encontrado");
            if (producto.StockDisponible < diferencia)
                return ResultadoOperacion.Invalido(
                    $"stock insuficiente para {producto.Nombre}: disponible {producto.StockDisponible}");
        }

        detalle.Cantidad = dto.Cantidad;
        detalle.Subtotal = dto.Cantidad * detalle.PrecioUnitario;
        await _repo.Actualizar(detalle);

        if (diferencia != 0)
            await _repo.AjustarStock(detalle.ProductoId, -diferencia);

        await _repo.RecalcularMontoVenta(idVenta);
        return ResultadoOperacion.Exito(detalle.Id);
    }

    // Quitar un renglón devuelve las unidades al stock.
    public async Task<bool> Eliminar(int idVenta, int id)
    {
        var detalle = await _repo.ObtenerPorId(id);
        if (detalle is null || detalle.VentaId != idVenta) return false;

        var idProducto = detalle.ProductoId;
        var cantidad = detalle.Cantidad;

        await _repo.Eliminar(detalle);
        await _repo.AjustarStock(idProducto, cantidad);
        await _repo.RecalcularMontoVenta(idVenta);
        return true;
    }
}

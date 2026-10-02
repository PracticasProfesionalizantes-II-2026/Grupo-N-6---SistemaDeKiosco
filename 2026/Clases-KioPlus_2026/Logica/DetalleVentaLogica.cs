using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class DetalleVentaLogica : IDetalleVentaLogica
{
    private readonly IDetalleVentaRepositorio _repo;
    private readonly ILoteRepositorio _repoLotes;
    private readonly ICuentaCorrienteClienteRepositorio _repoCuentas;

    public DetalleVentaLogica(
        IDetalleVentaRepositorio repo,
        ILoteRepositorio repoLotes,
        ICuentaCorrienteClienteRepositorio repoCuentas)
    {
        _repo = repo;
        _repoLotes = repoLotes;
        _repoCuentas = repoCuentas;
    }

    // Cambiar los renglones de una venta ya cerrada cambia su monto, así que la
    // deuda del cliente tiene que volver a cuadrar.
    private async Task ActualizarDeudaDelCliente(int idVenta)
    {
        var venta = await _repo.ObtenerVenta(idVenta);
        if (venta is null || !venta.Finalizada) return;

        await _repoCuentas.RecalcularDeuda(venta.CuentaCorrienteClienteId);
    }

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

    // Registra el renglón y descuenta las unidades de los lotes del producto
    // siguiendo FEFO. El stock del producto queda recalculado a partir de los lotes.
    public async Task<ResultadoOperacion> Crear(int idVenta, DetalleVentaCreateDto dto)
    {
        if (!await _repo.VentaExiste(idVenta))
            return ResultadoOperacion.NoEncontrado("venta no encontrada");

        var producto = await _repo.ObtenerProducto(dto.IdProducto);
        if (producto is null)
            return ResultadoOperacion.NoEncontrado("producto no encontrado");

        if (!await _repoLotes.ConsumirFefo(dto.IdProducto, dto.Cantidad))
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
        await _repo.RecalcularMontoVenta(idVenta);
        await ActualizarDeudaDelCliente(idVenta);
        return ResultadoOperacion.Exito(detalle.Id);
    }

    // Al cambiar la cantidad solo se mueve la diferencia contra los lotes.
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

            if (!await _repoLotes.ConsumirFefo(detalle.ProductoId, diferencia))
                return ResultadoOperacion.Invalido(
                    $"stock insuficiente para {producto.Nombre}: disponible {producto.StockDisponible}");
        }
        else if (diferencia < 0)
        {
            await _repoLotes.DevolverFefo(detalle.ProductoId, -diferencia);
        }

        detalle.Cantidad = dto.Cantidad;
        detalle.Subtotal = dto.Cantidad * detalle.PrecioUnitario;
        await _repo.Actualizar(detalle);
        await _repo.RecalcularMontoVenta(idVenta);
        await ActualizarDeudaDelCliente(idVenta);
        return ResultadoOperacion.Exito(detalle.Id);
    }

    // Quitar un renglón devuelve las unidades a los lotes de los que salieron.
    public async Task<bool> Eliminar(int idVenta, int id)
    {
        var detalle = await _repo.ObtenerPorId(id);
        if (detalle is null || detalle.VentaId != idVenta) return false;

        var idProducto = detalle.ProductoId;
        var cantidad = detalle.Cantidad;

        await _repo.Eliminar(detalle);
        await _repoLotes.DevolverFefo(idProducto, cantidad);
        await _repo.RecalcularMontoVenta(idVenta);
        await ActualizarDeudaDelCliente(idVenta);
        return true;
    }
}

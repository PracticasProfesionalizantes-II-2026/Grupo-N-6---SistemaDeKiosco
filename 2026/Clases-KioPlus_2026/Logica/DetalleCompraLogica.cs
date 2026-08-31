using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class DetalleCompraLogica : IDetalleCompraLogica
{
    private readonly IDetalleCompraRepositorio _repo;
    public DetalleCompraLogica(IDetalleCompraRepositorio repo) => _repo = repo;

    // En la entidad el precio se llama PrecioCompra; la doc lo expone como precioUnitario.
    private static DetalleCompraDto AMapa(DetalleCompra d) =>
        new(d.Id, d.CompraProveedorId, d.ProductoId, d.Cantidad, d.PrecioCompra, d.Subtotal);

    public async Task<IEnumerable<DetalleCompraListadoDto>> ObtenerPorCompra(int idCompra)
    {
        var filas = await _repo.ObtenerPorCompraConProducto(idCompra);
        return filas.Select(f => new DetalleCompraListadoDto(
            f.Detalle.Id, f.Detalle.ProductoId, f.Producto,
            f.Detalle.Cantidad, f.Detalle.PrecioCompra, f.Detalle.Subtotal));
    }

    public async Task<DetalleCompraDto?> ObtenerPorId(int id)
    {
        var d = await _repo.ObtenerPorId(id);
        return d is null ? null : AMapa(d);
    }

    // Registra el renglón e ingresa la mercadería al stock del producto.
    public async Task<ResultadoOperacion> Crear(int idCompra, DetalleCompraCreateDto dto)
    {
        if (!await _repo.CompraExiste(idCompra))
            return ResultadoOperacion.NoEncontrado("compra no encontrada");

        var producto = await _repo.ObtenerProducto(dto.IdProducto);
        if (producto is null)
            return ResultadoOperacion.NoEncontrado("producto no encontrado");

        var detalle = new DetalleCompra
        {
            CompraProveedorId = idCompra,
            ProductoId = dto.IdProducto,
            Cantidad = dto.Cantidad,
            PrecioCompra = dto.PrecioUnitario,
            Subtotal = dto.Cantidad * dto.PrecioUnitario
        };
        await _repo.Agregar(detalle);
        await _repo.AjustarStock(dto.IdProducto, dto.Cantidad);
        await _repo.RecalcularMontoCompra(idCompra);
        return ResultadoOperacion.Exito(detalle.Id);
    }

    // Revierte el ingreso anterior y aplica el nuevo, incluso si cambió el producto.
    public async Task<ResultadoOperacion> Actualizar(int id, DetalleCompraUpdateDto dto)
    {
        var detalle = await _repo.ObtenerPorId(id);
        if (detalle is null) return ResultadoOperacion.NoEncontrado("detalle no encontrado");

        var producto = await _repo.ObtenerProducto(dto.IdProducto);
        if (producto is null) return ResultadoOperacion.NoEncontrado("producto no encontrado");

        var compraAnterior = detalle.CompraProveedorId;
        var productoAnterior = detalle.ProductoId;
        var cantidadAnterior = detalle.Cantidad;

        detalle.CompraProveedorId = dto.IdCompra;
        detalle.ProductoId = dto.IdProducto;
        detalle.Cantidad = dto.Cantidad;
        detalle.PrecioCompra = dto.PrecioUnitario;
        detalle.Subtotal = dto.Cantidad * dto.PrecioUnitario;
        await _repo.Actualizar(detalle);

        if (productoAnterior == dto.IdProducto)
        {
            await _repo.AjustarStock(dto.IdProducto, dto.Cantidad - cantidadAnterior);
        }
        else
        {
            await _repo.AjustarStock(productoAnterior, -cantidadAnterior);
            await _repo.AjustarStock(dto.IdProducto, dto.Cantidad);
        }

        await _repo.RecalcularMontoCompra(dto.IdCompra);
        if (compraAnterior != dto.IdCompra)
            await _repo.RecalcularMontoCompra(compraAnterior);

        return ResultadoOperacion.Exito(detalle.Id);
    }

    // Quitar un renglón revierte el ingreso de mercadería.
    public async Task<bool> Eliminar(int id)
    {
        var detalle = await _repo.ObtenerPorId(id);
        if (detalle is null) return false;

        var idCompra = detalle.CompraProveedorId;
        var idProducto = detalle.ProductoId;
        var cantidad = detalle.Cantidad;

        await _repo.Eliminar(detalle);
        await _repo.AjustarStock(idProducto, -cantidad);
        await _repo.RecalcularMontoCompra(idCompra);
        return true;
    }
}

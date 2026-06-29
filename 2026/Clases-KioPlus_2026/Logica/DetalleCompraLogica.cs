using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;

public interface IDetalleCompraLogica
{
    Task<IEnumerable<DetalleCompraDto>> ObtenerPorCompra(int idCompra);
    Task<DetalleCompraDto?> ObtenerPorId(int id);
    Task<int?> Crear(int idCompra, DetalleCompraCreateDto dto);
    Task<bool> Actualizar(int id, DetalleCompraUpdateDto dto);
    Task<bool> Eliminar(int id);
}

public class DetalleCompraLogica : IDetalleCompraLogica
{
    private readonly IDetalleCompraRepositorio _repo;
    public DetalleCompraLogica(IDetalleCompraRepositorio repo) => _repo = repo;

    // En la entidad el precio se llama PrecioCompra; la doc lo expone como precioUnitario.
    private static DetalleCompraDto AMapa(DetalleCompra d) =>
        new(d.Id, d.CompraProveedorId, d.ProductoId, d.Cantidad, d.PrecioCompra, d.Subtotal);

    public async Task<IEnumerable<DetalleCompraDto>> ObtenerPorCompra(int idCompra)
    {
        var detalles = await _repo.ObtenerPorCompra(idCompra);
        return detalles.Select(AMapa);
    }

    public async Task<DetalleCompraDto?> ObtenerPorId(int id)
    {
        var d = await _repo.ObtenerPorId(id);
        return d is null ? null : AMapa(d);
    }

    // Devuelve null si la compra no existe
    public async Task<int?> Crear(int idCompra, DetalleCompraCreateDto dto)
    {
        if (!await _repo.CompraExiste(idCompra)) return null;

        var detalle = new DetalleCompra
        {
            CompraProveedorId = idCompra,
            ProductoId = dto.IdProducto,
            Cantidad = dto.Cantidad,
            PrecioCompra = dto.PrecioUnitario,
            Subtotal = dto.Cantidad * dto.PrecioUnitario
        };
        await _repo.Agregar(detalle);
        await _repo.RecalcularMontoCompra(idCompra);
        return detalle.Id;
    }

    public async Task<bool> Actualizar(int id, DetalleCompraUpdateDto dto)
    {
        var detalle = await _repo.ObtenerPorId(id);
        if (detalle is null) return false;

        var compraAnterior = detalle.CompraProveedorId;

        detalle.CompraProveedorId = dto.IdCompra;
        detalle.ProductoId = dto.IdProducto;
        detalle.Cantidad = dto.Cantidad;
        detalle.PrecioCompra = dto.PrecioUnitario;
        detalle.Subtotal = dto.Cantidad * dto.PrecioUnitario;
        await _repo.Actualizar(detalle);

        await _repo.RecalcularMontoCompra(dto.IdCompra);
        if (compraAnterior != dto.IdCompra)
            await _repo.RecalcularMontoCompra(compraAnterior);

        return true;
    }

    public async Task<bool> Eliminar(int id)
    {
        var detalle = await _repo.ObtenerPorId(id);
        if (detalle is null) return false;

        var idCompra = detalle.CompraProveedorId;
        await _repo.Eliminar(detalle);
        await _repo.RecalcularMontoCompra(idCompra);
        return true;
    }
}

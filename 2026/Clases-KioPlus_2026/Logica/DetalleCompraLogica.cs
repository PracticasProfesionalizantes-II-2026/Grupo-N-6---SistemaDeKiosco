using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class DetalleCompraLogica : IDetalleCompraLogica
{
    private readonly IDetalleCompraRepositorio _repo;
    private readonly ILoteRepositorio _repoLotes;
    private readonly ICajaRepositorio _repoCaja;
    private readonly IProductoProveedorRepositorio _repoProveedores;

    public DetalleCompraLogica(
        IDetalleCompraRepositorio repo,
        ILoteRepositorio repoLotes,
        ICajaRepositorio repoCaja,
        IProductoProveedorRepositorio repoProveedores)
    {
        _repo = repo;
        _repoLotes = repoLotes;
        _repoCaja = repoCaja;
        _repoProveedores = repoProveedores;
    }

    // Comprarle un producto a un proveedor deja constancia de que ese proveedor
    // lo vende. Si todavía no estaban asociados, se asocian con este precio.
    //
    // Si ya lo estaban, el precio de la asociación solo sube: un precio más alto
    // suele ser un aumento y conviene que quede como referencia, mientras que uno
    // más bajo suele ser un descuento puntual que no debería pisar el de lista.
    // El dueño siempre puede corregirlo a mano desde la pantalla de proveedores.
    private async Task RegistrarProveedorDelProducto(int idCompra, int idProducto, double precio)
    {
        var idProveedor = await _repo.ObtenerProveedorDeCompra(idCompra);
        if (idProveedor is null) return;

        var asociacion = await _repoProveedores.ObtenerPorProductoYProveedor(idProducto, idProveedor.Value);

        if (asociacion is null)
        {
            await _repoProveedores.Agregar(new ProductoProveedor
            {
                ProductoId = idProducto,
                ProveedorId = idProveedor.Value,
                PrecioCompra = precio
            });
            return;
        }

        if (precio > asociacion.PrecioCompra)
        {
            asociacion.PrecioCompra = precio;
            await _repoProveedores.Actualizar(asociacion);
        }
    }

    // En la entidad el precio se llama PrecioCompra; la doc lo expone como precioUnitario.
    private static DetalleCompraDto AMapa(DetalleCompra d, Lote? lote) =>
        new(d.Id, d.CompraProveedorId, d.ProductoId, d.Cantidad, d.PrecioCompra, d.Subtotal, lote?.Id);

    // Número de lote por defecto cuando el proveedor no trae uno impreso
    private static string NumeroPorDefecto(int idCompra, int idDetalle) => $"C{idCompra}-{idDetalle}";

    public async Task<IEnumerable<DetalleCompraListadoDto>> ObtenerPorCompra(int idCompra)
    {
        var filas = await _repo.ObtenerPorCompraConProducto(idCompra);
        return filas.Select(f => new DetalleCompraListadoDto(
            f.Detalle.Id, f.Detalle.ProductoId, f.Producto,
            f.Detalle.Cantidad, f.Detalle.PrecioCompra, f.Detalle.Subtotal,
            f.Lote?.NroLote, f.Lote?.FechaVencimiento));
    }

    public async Task<DetalleCompraDto?> ObtenerPorId(int id)
    {
        var d = await _repo.ObtenerPorId(id);
        if (d is null) return null;

        var lote = await _repoLotes.ObtenerPorDetalleCompra(id);
        return AMapa(d, lote);
    }

    // Registra el renglón y da de alta el lote que ingresa esa mercadería.
    // El stock del producto queda como la suma de sus lotes.
    public async Task<ResultadoOperacion> Crear(int idCompra, DetalleCompraCreateDto dto)
    {
        if (!await _repo.CompraExiste(idCompra))
            return ResultadoOperacion.NoEncontrado("compra no encontrada");

        var producto = await _repo.ObtenerProducto(dto.IdProducto);
        if (producto is null)
            return ResultadoOperacion.NoEncontrado("producto no encontrado");

        if (!Lote.VencimientoAdmitido(dto.FechaVencimiento))
            return ResultadoOperacion.Invalido(Lote.MensajeVencido);

        var detalle = new DetalleCompra
        {
            CompraProveedorId = idCompra,
            ProductoId = dto.IdProducto,
            Cantidad = dto.Cantidad,
            PrecioCompra = dto.PrecioUnitario,
            Subtotal = dto.Cantidad * dto.PrecioUnitario
        };
        await _repo.Agregar(detalle);

        await _repoLotes.Agregar(new Lote
        {
            ProductoId = dto.IdProducto,
            NroLote = string.IsNullOrWhiteSpace(dto.NroLote)
                ? NumeroPorDefecto(idCompra, detalle.Id)
                : dto.NroLote,
            FechaVencimiento = dto.FechaVencimiento,
            CantidadInicial = dto.Cantidad,
            Cantidad = dto.Cantidad,
            DetalleCompraId = detalle.Id
        });

        await _repoLotes.RecalcularStockProducto(dto.IdProducto);
        await _repo.RecalcularMontoCompra(idCompra);
        await _repoCaja.RegistrarEgreso(detalle.Subtotal);
        await RegistrarProveedorDelProducto(idCompra, dto.IdProducto, dto.PrecioUnitario);
        return ResultadoOperacion.Exito(detalle.Id);
    }

    // Reescribe el renglón y su lote. Si cambió el producto, el lote se muda con él.
    public async Task<ResultadoOperacion> Actualizar(int id, DetalleCompraUpdateDto dto)
    {
        var detalle = await _repo.ObtenerPorId(id);
        if (detalle is null) return ResultadoOperacion.NoEncontrado("detalle no encontrado");

        var producto = await _repo.ObtenerProducto(dto.IdProducto);
        if (producto is null) return ResultadoOperacion.NoEncontrado("producto no encontrado");

        var lote = await _repoLotes.ObtenerPorDetalleCompra(id);
        if (!Lote.VencimientoAdmitido(dto.FechaVencimiento, lote?.FechaVencimiento))
            return ResultadoOperacion.Invalido(Lote.MensajeVencido);

        var compraAnterior = detalle.CompraProveedorId;
        var productoAnterior = detalle.ProductoId;
        var subtotalAnterior = detalle.Subtotal;

        detalle.CompraProveedorId = dto.IdCompra;
        detalle.ProductoId = dto.IdProducto;
        detalle.Cantidad = dto.Cantidad;
        detalle.PrecioCompra = dto.PrecioUnitario;
        detalle.Subtotal = dto.Cantidad * dto.PrecioUnitario;
        await _repo.Actualizar(detalle);

        if (lote is null)
        {
            await _repoLotes.Agregar(new Lote
            {
                ProductoId = dto.IdProducto,
                NroLote = string.IsNullOrWhiteSpace(dto.NroLote)
                    ? NumeroPorDefecto(dto.IdCompra, id)
                    : dto.NroLote,
                FechaVencimiento = dto.FechaVencimiento,
                CantidadInicial = dto.Cantidad,
                Cantidad = dto.Cantidad,
                DetalleCompraId = id
            });
        }
        else
        {
            // Lo ya vendido de este lote se respeta: solo se corre la diferencia.
            var vendido = lote.CantidadInicial - lote.Cantidad;

            lote.ProductoId = dto.IdProducto;
            lote.NroLote = string.IsNullOrWhiteSpace(dto.NroLote)
                ? NumeroPorDefecto(dto.IdCompra, id)
                : dto.NroLote;
            lote.FechaVencimiento = dto.FechaVencimiento;
            lote.CantidadInicial = dto.Cantidad;
            lote.Cantidad = Math.Max(0, dto.Cantidad - vendido);
            await _repoLotes.Actualizar(lote);
        }

        if (productoAnterior != dto.IdProducto)
            await _repoLotes.RecalcularStockProducto(productoAnterior);
        await _repoLotes.RecalcularStockProducto(dto.IdProducto);

        await _repo.RecalcularMontoCompra(dto.IdCompra);
        if (compraAnterior != dto.IdCompra)
            await _repo.RecalcularMontoCompra(compraAnterior);

        // La caja solo se mueve por la diferencia de plata del renglón
        var diferencia = detalle.Subtotal - subtotalAnterior;
        if (diferencia > 0) await _repoCaja.RegistrarEgreso(diferencia);
        else if (diferencia < 0) await _repoCaja.RegistrarIngreso(-diferencia);

        await RegistrarProveedorDelProducto(dto.IdCompra, dto.IdProducto, dto.PrecioUnitario);
        return ResultadoOperacion.Exito(detalle.Id);
    }

    // Quitar un renglón da de baja el lote que había ingresado.
    public async Task<bool> Eliminar(int id)
    {
        var detalle = await _repo.ObtenerPorId(id);
        if (detalle is null) return false;

        var idCompra = detalle.CompraProveedorId;
        var idProducto = detalle.ProductoId;
        var subtotal = detalle.Subtotal;

        await _repoLotes.EliminarPorDetallesCompra(new[] { id });
        await _repo.Eliminar(detalle);
        await _repoLotes.RecalcularStockProducto(idProducto);
        await _repo.RecalcularMontoCompra(idCompra);
        await _repoCaja.RegistrarIngreso(subtotal);
        return true;
    }
}

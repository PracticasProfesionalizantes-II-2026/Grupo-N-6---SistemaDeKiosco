using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class LoteLogica : ILoteLogica
{
    private readonly ILoteRepositorio _repo;
    public LoteLogica(ILoteRepositorio repo) => _repo = repo;

    private static LoteDto AMapa(Lote l) =>
        new(l.Id, l.ProductoId, l.NroLote, l.FechaVencimiento, l.CantidadInicial, l.Cantidad);

    public async Task<IEnumerable<LoteDto>> ObtenerPorProducto(int idProducto)
    {
        var lotes = await _repo.ObtenerPorProducto(idProducto);
        return lotes.Select(AMapa);
    }

    public async Task<LoteDto?> ObtenerPorId(int id)
    {
        var l = await _repo.ObtenerPorId(id);
        return l is null ? null : AMapa(l);
    }

    // Dar de alta un lote es la forma de ingresar mercadería al stock.
    public async Task<ResultadoOperacion> Crear(int idProducto, LoteCreateDto dto)
    {
        if (!await _repo.ProductoExiste(idProducto))
            return ResultadoOperacion.NoEncontrado("producto no encontrado");

        if (!Lote.VencimientoAdmitido(dto.FechaVencimiento))
            return ResultadoOperacion.Invalido(Lote.MensajeVencido);

        var lote = new Lote
        {
            ProductoId = idProducto,
            NroLote = dto.NroLote,
            FechaVencimiento = dto.FechaVencimiento,
            CantidadInicial = dto.Cantidad,
            Cantidad = dto.Cantidad
        };
        await _repo.Agregar(lote);
        await _repo.RecalcularStockProducto(idProducto);
        return ResultadoOperacion.Exito(lote.Id);
    }

    // Editar un lote corrige la cantidad que queda disponible. La cantidad inicial
    // solo crece, para seguir sirviendo de tope cuando se anula una venta.
    public async Task<ResultadoOperacion> Actualizar(int id, LoteCreateDto dto)
    {
        var lote = await _repo.ObtenerPorId(id);
        if (lote is null) return ResultadoOperacion.NoEncontrado("lote no encontrado");

        if (!Lote.VencimientoAdmitido(dto.FechaVencimiento, lote.FechaVencimiento))
            return ResultadoOperacion.Invalido(Lote.MensajeVencido);

        lote.NroLote = dto.NroLote;
        lote.FechaVencimiento = dto.FechaVencimiento;
        lote.Cantidad = dto.Cantidad;
        lote.CantidadInicial = Math.Max(lote.CantidadInicial, dto.Cantidad);
        await _repo.Actualizar(lote);
        await _repo.RecalcularStockProducto(lote.ProductoId);
        return ResultadoOperacion.Exito(lote.Id);
    }

    public async Task<bool> Eliminar(int id)
    {
        var lote = await _repo.ObtenerPorId(id);
        if (lote is null) return false;

        var idProducto = lote.ProductoId;
        await _repo.Eliminar(lote);
        await _repo.RecalcularStockProducto(idProducto);
        return true;
    }
}

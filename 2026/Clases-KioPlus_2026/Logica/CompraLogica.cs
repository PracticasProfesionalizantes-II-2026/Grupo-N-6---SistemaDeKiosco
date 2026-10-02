using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class CompraLogica : ICompraLogica
{
    private readonly ICompraRepositorio _repo;
    private readonly IDetalleCompraRepositorio _repoDetalles;
    private readonly ILoteRepositorio _repoLotes;
    private readonly ICajaRepositorio _repoCaja;

    public CompraLogica(
        ICompraRepositorio repo,
        IDetalleCompraRepositorio repoDetalles,
        ILoteRepositorio repoLotes,
        ICajaRepositorio repoCaja)
    {
        _repo = repo;
        _repoDetalles = repoDetalles;
        _repoLotes = repoLotes;
        _repoCaja = repoCaja;
    }

    private static CompraDto AMapa(CompraProveedor c) =>
        new(c.Id, c.FechaHora, c.ProveedorId, c.MontoTotal);

    public async Task<IEnumerable<CompraListadoDto>> ObtenerTodas(
        DateTime? fechaDesde, DateTime? fechaHasta, int? idProveedor)
    {
        var filas = await _repo.ObtenerTodasConProveedor(fechaDesde, fechaHasta, idProveedor);
        return filas.Select(f => new CompraListadoDto(
            f.Compra.Id, f.Compra.FechaHora, f.Compra.ProveedorId, f.Proveedor, f.Compra.MontoTotal));
    }

    public async Task<CompraDto?> ObtenerPorId(int id)
    {
        var c = await _repo.ObtenerPorId(id);
        return c is null ? null : AMapa(c);
    }

    // Devuelve null si el proveedor no existe
    public async Task<int?> Crear(CompraCreateDto dto)
    {
        if (!await _repo.ProveedorExiste(dto.IdProveedor)) return null;

        var compra = new CompraProveedor
        {
            FechaHora = dto.FechaHora == default ? DateTime.Now : dto.FechaHora,
            ProveedorId = dto.IdProveedor,
            MontoTotal = 0
        };
        await _repo.Agregar(compra);
        return compra.Id;
    }

    public async Task<bool> Actualizar(int id, CompraCreateDto dto)
    {
        var compra = await _repo.ObtenerPorId(id);
        if (compra is null) return false;

        compra.FechaHora = dto.FechaHora;
        compra.ProveedorId = dto.IdProveedor;
        await _repo.Actualizar(compra);
        return true;
    }

    // Anular una compra da de baja los lotes que había ingresado, recalcula el
    // stock de los productos afectados y devuelve la plata a la caja.
    public async Task<bool> Eliminar(int id)
    {
        var compra = await _repo.ObtenerPorId(id);
        if (compra is null) return false;

        var detalles = (await _repoDetalles.ObtenerPorCompra(id)).ToList();
        var productos = detalles.Select(d => d.ProductoId).Distinct().ToList();

        await _repoLotes.EliminarPorDetallesCompra(detalles.Select(d => d.Id));
        await _repo.Eliminar(compra);

        foreach (var idProducto in productos)
            await _repoLotes.RecalcularStockProducto(idProducto);

        await _repoCaja.RegistrarIngreso(compra.MontoTotal);
        return true;
    }
}

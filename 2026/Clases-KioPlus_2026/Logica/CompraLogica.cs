using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;

public interface ICompraLogica
{
    Task<IEnumerable<CompraDto>> ObtenerTodas();
    Task<CompraDto?> ObtenerPorId(int id);
    Task<int?> Crear(CompraCreateDto dto);
    Task<bool> Actualizar(int id, CompraCreateDto dto);
    Task<bool> Eliminar(int id);
}

public class CompraLogica : ICompraLogica
{
    private readonly ICompraRepositorio _repo;
    public CompraLogica(ICompraRepositorio repo) => _repo = repo;

    private static CompraDto AMapa(CompraProveedor c) =>
        new(c.Id, c.FechaHora, c.ProveedorId, c.MontoTotal);

    public async Task<IEnumerable<CompraDto>> ObtenerTodas()
    {
        var compras = await _repo.ObtenerTodas();
        return compras.Select(AMapa);
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
            FechaHora = dto.FechaHora,
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

    public async Task<bool> Eliminar(int id)
    {
        var compra = await _repo.ObtenerPorId(id);
        if (compra is null) return false;

        await _repo.Eliminar(compra);
        return true;
    }
}

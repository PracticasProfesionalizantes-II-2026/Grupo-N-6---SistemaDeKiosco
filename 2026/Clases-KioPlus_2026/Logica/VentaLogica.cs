using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;

public interface IVentaLogica
{
    Task<IEnumerable<VentaDto>> ObtenerTodas();
    Task<IEnumerable<VentaFiltroDto>> Filtrar(
        DateTime? fechaHora, int? idUsuario, int? idCliente,
        double? importeMayorA, double? importeMenorA);
    Task<VentaDto?> ObtenerPorId(int id);
    Task<int?> Crear(VentaCreateDto dto);
    Task<bool> Actualizar(int id, VentaCreateDto dto);
    Task<bool> Eliminar(int id);
}

public class VentaLogica : IVentaLogica
{
    private readonly IVentaRepositorio _repo;
    public VentaLogica(IVentaRepositorio repo) => _repo = repo;

    private static VentaDto AMapa(Venta v) =>
        new(v.Id, v.UsuarioId, v.FechaHora, v.MontoTotal, v.CuentaCorrienteClienteId,
            v.FormaPago, v.FechaPago, v.Estado);

    private static Venta.EstadoVenta EstadoSegunPago(Venta.FormaDePago forma) =>
        forma == Venta.FormaDePago.PagadoAlMomento
            ? Venta.EstadoVenta.Pagado
            : Venta.EstadoVenta.NoPagado;

    public async Task<IEnumerable<VentaDto>> ObtenerTodas()
    {
        var ventas = await _repo.ObtenerTodas();
        return ventas.Select(AMapa);
    }

    public async Task<IEnumerable<VentaFiltroDto>> Filtrar(
        DateTime? fechaHora, int? idUsuario, int? idCliente,
        double? importeMayorA, double? importeMenorA)
    {
        var ventas = await _repo.Filtrar(fechaHora, idUsuario, idCliente, importeMayorA, importeMenorA);
        return ventas.Select(v => new VentaFiltroDto(
            v.FechaHora, v.UsuarioId, v.CuentaCorrienteClienteId, v.MontoTotal));
    }

    public async Task<VentaDto?> ObtenerPorId(int id)
    {
        var v = await _repo.ObtenerPorId(id);
        return v is null ? null : AMapa(v);
    }

    // Devuelve null si el usuario o la cuenta corriente no existen
    public async Task<int?> Crear(VentaCreateDto dto)
    {
        if (!await _repo.UsuarioExiste(dto.IdUsuario)) return null;
        if (!await _repo.CuentaExiste(dto.IdCuentaCorrienteCliente)) return null;

        var venta = new Venta
        {
            FechaHora = dto.FechaHora,
            UsuarioId = dto.IdUsuario,
            CuentaCorrienteClienteId = dto.IdCuentaCorrienteCliente,
            FormaPago = dto.FormaPago,
            FechaPago = dto.FechaPago,
            MontoTotal = 0,
            Estado = EstadoSegunPago(dto.FormaPago)
        };
        await _repo.Agregar(venta);
        return venta.Id;
    }

    public async Task<bool> Actualizar(int id, VentaCreateDto dto)
    {
        var venta = await _repo.ObtenerPorId(id);
        if (venta is null) return false;

        venta.FechaHora = dto.FechaHora;
        venta.UsuarioId = dto.IdUsuario;
        venta.CuentaCorrienteClienteId = dto.IdCuentaCorrienteCliente;
        venta.FormaPago = dto.FormaPago;
        venta.FechaPago = dto.FechaPago;
        venta.Estado = EstadoSegunPago(dto.FormaPago);
        await _repo.Actualizar(venta);
        return true;
    }

    public async Task<bool> Eliminar(int id)
    {
        var venta = await _repo.ObtenerPorId(id);
        if (venta is null) return false;

        await _repo.Eliminar(venta);
        return true;
    }
}

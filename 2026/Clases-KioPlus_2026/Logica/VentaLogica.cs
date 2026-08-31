using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class VentaLogica : IVentaLogica
{
    private readonly IVentaRepositorio _repo;
    private readonly ICuentaCorrienteClienteRepositorio _repoCuentas;

    public VentaLogica(IVentaRepositorio repo, ICuentaCorrienteClienteRepositorio repoCuentas)
    {
        _repo = repo;
        _repoCuentas = repoCuentas;
    }

    private static VentaDto AMapa(Venta v) =>
        new(v.Id, v.UsuarioId, v.FechaHora, v.MontoTotal, v.CuentaCorrienteClienteId,
            v.FormaPago, v.FechaPago, v.Estado);

    private static Venta.EstadoVenta EstadoSegunPago(Venta.FormaDePago forma) =>
        forma == Venta.FormaDePago.PagadoAlMomento
            ? Venta.EstadoVenta.Pagado
            : Venta.EstadoVenta.NoPagado;

    public async Task<IEnumerable<VentaListadoDto>> ObtenerTodas(
        DateTime? fechaDesde, DateTime? fechaHasta, int? idUsuario, int? idCliente,
        double? importeMayorA, double? importeMenorA)
    {
        var filas = await _repo.ObtenerTodasConNombres(
            fechaDesde, fechaHasta, idUsuario, idCliente, importeMayorA, importeMenorA);

        return filas.Select(f => new VentaListadoDto(
            f.Venta.Id, f.Venta.FechaHora, f.Venta.UsuarioId, f.Vendedor,
            f.Venta.CuentaCorrienteClienteId, f.Cliente, f.Venta.MontoTotal,
            f.Venta.FormaPago, f.Venta.Estado));
    }

    public async Task<VentaDto?> ObtenerPorId(int id)
    {
        var v = await _repo.ObtenerPorId(id);
        return v is null ? null : AMapa(v);
    }

    // Una venta sin cliente explícito se registra contra Consumidor Final.
    // Una venta a cuenta corriente exige un cliente real.
    public async Task<ResultadoOperacion> Crear(VentaCreateDto dto)
    {
        if (!await _repo.UsuarioExiste(dto.IdUsuario))
            return ResultadoOperacion.NoEncontrado("usuario no encontrado");

        var idCliente = dto.IdCuentaCorrienteCliente <= 0
            ? CuentaCorrienteCliente.IdConsumidorFinal
            : dto.IdCuentaCorrienteCliente;

        if (!await _repo.CuentaExiste(idCliente))
            return ResultadoOperacion.NoEncontrado("cuenta corriente no encontrada");

        if (dto.FormaPago == Venta.FormaDePago.CuentaCorriente &&
            idCliente == CuentaCorrienteCliente.IdConsumidorFinal)
            return ResultadoOperacion.Invalido(
                "una venta en cuenta corriente necesita un cliente registrado");

        var venta = new Venta
        {
            FechaHora = dto.FechaHora == default ? DateTime.Now : dto.FechaHora,
            UsuarioId = dto.IdUsuario,
            CuentaCorrienteClienteId = idCliente,
            FormaPago = dto.FormaPago,
            FechaPago = dto.FechaPago,
            MontoTotal = 0,
            Estado = EstadoSegunPago(dto.FormaPago)
        };
        await _repo.Agregar(venta);
        return ResultadoOperacion.Exito(venta.Id);
    }

    public async Task<bool> Actualizar(int id, VentaCreateDto dto)
    {
        var venta = await _repo.ObtenerPorId(id);
        if (venta is null) return false;

        venta.FechaHora = dto.FechaHora;
        venta.UsuarioId = dto.IdUsuario;
        venta.CuentaCorrienteClienteId = dto.IdCuentaCorrienteCliente <= 0
            ? CuentaCorrienteCliente.IdConsumidorFinal
            : dto.IdCuentaCorrienteCliente;
        venta.FormaPago = dto.FormaPago;
        venta.FechaPago = dto.FechaPago;
        venta.Estado = EstadoSegunPago(dto.FormaPago);
        await _repo.Actualizar(venta);
        return true;
    }

    // Cierra la venta: si fue en cuenta corriente, impacta el saldo del cliente.
    // El front lo llama una sola vez, al confirmar "Finalizar venta".
    public async Task<ResultadoOperacion> Finalizar(int id)
    {
        var venta = await _repo.ObtenerPorId(id);
        if (venta is null) return ResultadoOperacion.NoEncontrado("venta no encontrada");

        if (venta.MontoTotal <= 0)
            return ResultadoOperacion.Invalido("la venta no tiene productos cargados");

        if (venta.FormaPago == Venta.FormaDePago.CuentaCorriente &&
            venta.CuentaCorrienteClienteId != CuentaCorrienteCliente.IdConsumidorFinal)
        {
            await _repoCuentas.AjustarDeuda(venta.CuentaCorrienteClienteId, venta.MontoTotal);
        }

        return ResultadoOperacion.Exito(venta.Id);
    }

    public async Task<bool> Eliminar(int id)
    {
        var venta = await _repo.ObtenerPorId(id);
        if (venta is null) return false;

        await _repo.Eliminar(venta);
        return true;
    }
}

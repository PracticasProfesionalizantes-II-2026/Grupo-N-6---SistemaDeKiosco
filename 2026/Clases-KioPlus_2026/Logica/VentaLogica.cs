using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class VentaLogica : IVentaLogica
{
    private readonly IVentaRepositorio _repo;
    private readonly ICuentaCorrienteClienteRepositorio _repoCuentas;
    private readonly IDetalleVentaRepositorio _repoDetalles;
    private readonly ILoteRepositorio _repoLotes;
    private readonly ICajaRepositorio _repoCaja;

    public VentaLogica(
        IVentaRepositorio repo,
        ICuentaCorrienteClienteRepositorio repoCuentas,
        IDetalleVentaRepositorio repoDetalles,
        ILoteRepositorio repoLotes,
        ICajaRepositorio repoCaja)
    {
        _repo = repo;
        _repoCuentas = repoCuentas;
        _repoDetalles = repoDetalles;
        _repoLotes = repoLotes;
        _repoCaja = repoCaja;
    }

    private static VentaDto AMapa(Venta v) =>
        new(v.Id, v.UsuarioId, v.FechaHora, v.MontoTotal, v.CuentaCorrienteClienteId,
            v.FormaPago, v.FechaPago, v.Estado, v.Finalizada, v.MontoPagado);

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

    public async Task<IEnumerable<VentaAdeudadaDto>> ObtenerAdeudadasPorCliente(int idCliente)
    {
        var filas = await _repo.ObtenerAdeudadasPorCliente(idCliente);
        return filas.Select(f => new VentaAdeudadaDto(
            f.Venta.Id, f.Venta.FechaHora, f.Vendedor,
            f.Venta.MontoTotal, f.Venta.MontoPagado, f.Venta.SaldoPendiente));
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
            Estado = EstadoSegunPago(dto.FormaPago),
            Finalizada = false,
            MontoPagado = 0
        };
        await _repo.Agregar(venta);
        return ResultadoOperacion.Exito(venta.Id);
    }

    // Editar una venta cerrada puede cambiar el cliente o la forma de pago. Se
    // deshace primero lo que la venta le debía al cliente anterior y lo que había
    // entrado a la caja, y recién después se aplica el estado nuevo.
    public async Task<bool> Actualizar(int id, VentaCreateDto dto)
    {
        var venta = await _repo.ObtenerPorId(id);
        if (venta is null) return false;

        var clienteAnterior = venta.CuentaCorrienteClienteId;
        var cobradoAnterior = venta.Finalizada ? venta.MontoPagado : 0;

        if (cobradoAnterior > 0)
            await _repoCaja.RegistrarEgreso(cobradoAnterior);

        venta.FechaHora = dto.FechaHora;
        venta.UsuarioId = dto.IdUsuario;
        venta.CuentaCorrienteClienteId = dto.IdCuentaCorrienteCliente <= 0
            ? CuentaCorrienteCliente.IdConsumidorFinal
            : dto.IdCuentaCorrienteCliente;
        venta.FormaPago = dto.FormaPago;
        venta.FechaPago = dto.FechaPago;
        venta.Estado = EstadoSegunPago(dto.FormaPago);
        venta.MontoPagado = venta.Finalizada && dto.FormaPago == Venta.FormaDePago.PagadoAlMomento
            ? venta.MontoTotal
            : 0;
        await _repo.Actualizar(venta);

        if (venta.MontoPagado > 0)
            await _repoCaja.RegistrarIngreso(venta.MontoPagado);

        await _repoCuentas.RecalcularDeuda(clienteAnterior);
        if (venta.CuentaCorrienteClienteId != clienteAnterior)
            await _repoCuentas.RecalcularDeuda(venta.CuentaCorrienteClienteId);

        return true;
    }

    // Cierra la venta: si se cobró al momento entra a la caja, y si fue en cuenta
    // corriente impacta el saldo del cliente. El front lo llama una sola vez,
    // al confirmar "Finalizar venta"; una segunda llamada se rechaza.
    public async Task<ResultadoOperacion> Finalizar(int id)
    {
        var venta = await _repo.ObtenerPorId(id);
        if (venta is null) return ResultadoOperacion.NoEncontrado("venta no encontrada");

        if (venta.Finalizada)
            return ResultadoOperacion.Invalido("la venta ya fue finalizada");

        if (venta.MontoTotal <= 0)
            return ResultadoOperacion.Invalido("la venta no tiene productos cargados");

        venta.Finalizada = true;

        if (venta.FormaPago == Venta.FormaDePago.PagadoAlMomento)
        {
            venta.Estado = Venta.EstadoVenta.Pagado;
            venta.FechaPago = venta.FechaHora;
            venta.MontoPagado = venta.MontoTotal;
            await _repo.Actualizar(venta);
            await _repoCaja.RegistrarIngreso(venta.MontoTotal);
        }
        else
        {
            venta.Estado = Venta.EstadoVenta.NoPagado;
            await _repo.Actualizar(venta);
            await _repoCuentas.RecalcularDeuda(venta.CuentaCorrienteClienteId);
        }

        return ResultadoOperacion.Exito(venta.Id);
    }

    // Cobro de una venta adeudada: baja la deuda del cliente y entra a la caja.
    // La fecha de pago no puede ser futura ni anterior a la de la venta.
    public async Task<ResultadoOperacion> RegistrarPago(int id, PagoVentaDto dto)
    {
        var venta = await _repo.ObtenerPorId(id);
        if (venta is null) return ResultadoOperacion.NoEncontrado("venta no encontrada");

        if (venta.Estado == Venta.EstadoVenta.Pagado)
            return ResultadoOperacion.Invalido("la venta ya está pagada");

        if (venta.FormaPago != Venta.FormaDePago.CuentaCorriente)
            return ResultadoOperacion.Invalido("la venta no es en cuenta corriente");

        var fechaPago = dto.FechaPago ?? DateTime.Now;

        if (fechaPago.Date > DateTime.Now.Date)
            return ResultadoOperacion.Invalido("no se puede registrar una fecha de pago del futuro");

        if (fechaPago.Date < venta.FechaHora.Date)
            return ResultadoOperacion.Invalido(
                "la fecha de pago debe ser posterior a la fecha en la que se realizó la venta");

        // Puede quedar un saldo menor al total si antes se imputó un pago a cuenta
        var saldo = venta.SaldoPendiente;

        venta.Estado = Venta.EstadoVenta.Pagado;
        venta.FechaPago = fechaPago;
        venta.MontoPagado = venta.MontoTotal;
        await _repo.Actualizar(venta);

        await _repoCuentas.RecalcularDeuda(venta.CuentaCorrienteClienteId);
        await _repoCaja.RegistrarIngreso(saldo);
        return ResultadoOperacion.Exito(venta.Id);
    }

    // Anular una venta devuelve la mercadería a sus lotes y deshace el impacto
    // que la venta había tenido en la caja o en la cuenta corriente del cliente.
    public async Task<bool> Eliminar(int id)
    {
        var venta = await _repo.ObtenerPorId(id);
        if (venta is null) return false;

        var detalles = (await _repoDetalles.ObtenerPorVenta(id)).ToList();

        var idCliente = venta.CuentaCorrienteClienteId;

        // Lo que el cliente ya había pagado de esta venta se le devuelve desde la caja
        if (venta.Finalizada && venta.MontoPagado > 0)
            await _repoCaja.RegistrarEgreso(venta.MontoPagado);

        await _repo.Eliminar(venta);
        await _repoCuentas.RecalcularDeuda(idCliente);

        foreach (var detalle in detalles)
            await _repoLotes.DevolverFefo(detalle.ProductoId, detalle.Cantidad);

        return true;
    }
}

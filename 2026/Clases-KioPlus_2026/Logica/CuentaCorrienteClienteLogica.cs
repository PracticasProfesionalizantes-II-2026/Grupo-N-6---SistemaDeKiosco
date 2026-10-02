using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class CuentaCorrienteClienteLogica : ICuentaCorrienteClienteLogica
{
    private readonly ICuentaCorrienteClienteRepositorio _repo;
    private readonly IVentaRepositorio _repoVentas;
    private readonly ICajaRepositorio _repoCaja;

    public CuentaCorrienteClienteLogica(
        ICuentaCorrienteClienteRepositorio repo,
        IVentaRepositorio repoVentas,
        ICajaRepositorio repoCaja)
    {
        _repo = repo;
        _repoVentas = repoVentas;
        _repoCaja = repoCaja;
    }

    private static CuentaCorrienteClienteDto AMapa(CuentaCorrienteCliente c) =>
        new(c.Id, c.Nombre, c.Apellido, c.Dni, c.Telefono, c.Direccion,
            c.CorreoElectronico, c.MontoAdeudado, c.Estado);

    // El estado nunca se carga a mano: se deduce del saldo adeudado.
    private static CuentaCorrienteCliente.EstadoDeuda EstadoSegunDeuda(double monto) =>
        monto > 0
            ? CuentaCorrienteCliente.EstadoDeuda.Moroso
            : CuentaCorrienteCliente.EstadoDeuda.AlDia;

    public async Task<IEnumerable<CuentaCorrienteClienteDto>> ObtenerTodas(
        string? nombre, string? apellido, int? dni,
        CuentaCorrienteCliente.EstadoDeuda? estado,
        double? montoAdeudadoMin, double? montoAdeudadoMax)
    {
        var cuentas = await _repo.ObtenerTodas(nombre, apellido, dni, estado, montoAdeudadoMin, montoAdeudadoMax);
        return cuentas.Select(AMapa);
    }

    public async Task<CuentaCorrienteClienteDto?> ObtenerPorId(int id)
    {
        var c = await _repo.ObtenerPorId(id);
        return c is null ? null : AMapa(c);
    }

    public async Task<int> Crear(CuentaCorrienteClienteCreateDto dto)
    {
        var cuenta = new CuentaCorrienteCliente
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Dni = dto.Dni,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            CorreoElectronico = dto.CorreoElectronico,
            SaldoInicial = dto.MontoAdeudado,
            MontoAdeudado = dto.MontoAdeudado,
            Estado = EstadoSegunDeuda(dto.MontoAdeudado)
        };
        await _repo.Agregar(cuenta);
        return cuenta.Id;
    }

    public async Task<bool> Actualizar(int id, CuentaCorrienteClienteCreateDto dto)
    {
        var cuenta = await _repo.ObtenerPorId(id);
        if (cuenta is null) return false;

        cuenta.Nombre = dto.Nombre;
        cuenta.Apellido = dto.Apellido;
        cuenta.Dni = dto.Dni;
        cuenta.Telefono = dto.Telefono;
        cuenta.Direccion = dto.Direccion;
        cuenta.CorreoElectronico = dto.CorreoElectronico;
        // Lo que se edita a mano es el saldo que no viene de ninguna venta; el
        // monto adeudado se reconstruye a partir de él y de las ventas impagas.
        cuenta.SaldoInicial = dto.MontoAdeudado;
        await _repo.Actualizar(cuenta);
        await _repo.RecalcularDeuda(id);
        return true;
    }

    // Consumidor Final es la cuenta por defecto de las ventas: no se puede borrar.
    public async Task<ResultadoOperacion> Eliminar(int id)
    {
        if (id == CuentaCorrienteCliente.IdConsumidorFinal)
            return ResultadoOperacion.Invalido("no se puede eliminar la cuenta Consumidor Final");

        var cuenta = await _repo.ObtenerPorId(id);
        if (cuenta is null) return ResultadoOperacion.NoEncontrado("cuenta corriente no encontrada");

        await _repo.Eliminar(cuenta);
        return ResultadoOperacion.Exito(id);
    }

    // Pago a cuenta por un monto libre. El pago se imputa a las ventas adeudadas
    // empezando por la más antigua: las que quedan cubiertas pasan a "Pagado" y
    // salen de la lista, y si sobra un resto queda como pago parcial de la
    // siguiente. Así el saldo del cliente siempre coincide con lo que se ve.
    public async Task<ResultadoOperacion> RegistrarPago(int id, PagoCuentaCorrienteDto dto)
    {
        var cuenta = await _repo.ObtenerPorId(id);
        if (cuenta is null) return ResultadoOperacion.NoEncontrado("cuenta corriente no encontrada");

        if (cuenta.MontoAdeudado <= 0)
            return ResultadoOperacion.Invalido("la cuenta no tiene deuda pendiente");

        if (dto.Monto > cuenta.MontoAdeudado)
            return ResultadoOperacion.Invalido(
                $"el pago supera la deuda actual de {cuenta.MontoAdeudado:0.##}");

        // Lo que no entra en ninguna venta baja el saldo cargado a mano
        var sinImputar = await ImputarAVentas(id, dto.Monto, DateTime.Now);
        await _repo.DescontarSaldoInicial(id, sinImputar);
        await _repo.RecalcularDeuda(id);
        await _repoCaja.RegistrarIngreso(dto.Monto);
        return ResultadoOperacion.Exito(id);
    }

    // Reparte un pago entre las ventas adeudadas, de la más antigua a la más
    // reciente. Devuelve lo que no se pudo imputar a ninguna venta, que es el
    // saldo que se había cargado a mano al dar de alta la cuenta.
    private async Task<double> ImputarAVentas(int idCliente, double monto, DateTime fechaPago)
    {
        var restante = monto;
        var adeudadas = await _repoVentas.ObtenerAdeudadasPorCliente(idCliente);

        foreach (var (venta, _) in adeudadas)
        {
            if (restante <= 0) break;

            var imputado = Math.Min(venta.SaldoPendiente, restante);
            if (imputado <= 0) continue;

            venta.MontoPagado += imputado;
            restante -= imputado;

            if (venta.SaldoPendiente <= 0)
            {
                venta.Estado = Venta.EstadoVenta.Pagado;
                venta.FechaPago = fechaPago;
            }

            await _repoVentas.Actualizar(venta);
        }

        return restante;
    }

    // Cancela la deuda completa: marca como pagadas todas las ventas adeudadas y
    // deja el saldo del cliente en cero. La fecha de pago no puede ser futura ni
    // anterior a la de la última venta que el cliente adeuda.
    public async Task<ResultadoOperacion> PagarDeudaTotal(int id, PagoDeudaTotalDto dto)
    {
        var cuenta = await _repo.ObtenerPorId(id);
        if (cuenta is null) return ResultadoOperacion.NoEncontrado("cuenta corriente no encontrada");

        if (cuenta.MontoAdeudado <= 0)
            return ResultadoOperacion.Invalido("la cuenta no tiene deuda pendiente");

        var fechaPago = dto.FechaPago ?? DateTime.Now;

        if (fechaPago.Date > DateTime.Now.Date)
            return ResultadoOperacion.Invalido("no se puede registrar una fecha de pago del futuro");

        var adeudadas = (await _repoVentas.ObtenerAdeudadasPorCliente(id)).ToList();

        if (adeudadas.Count > 0 &&
            fechaPago.Date < adeudadas.Max(a => a.Venta.FechaHora).Date)
            return ResultadoOperacion.Invalido(
                "la fecha de pago debe ser posterior a la fecha en la que se realizó la última venta adeudada");

        foreach (var (venta, _) in adeudadas)
        {
            venta.MontoPagado = venta.MontoTotal;
            venta.Estado = Venta.EstadoVenta.Pagado;
            venta.FechaPago = fechaPago;
            await _repoVentas.Actualizar(venta);
        }

        var total = cuenta.MontoAdeudado;
        await _repo.DescontarSaldoInicial(id, cuenta.SaldoInicial);
        await _repo.RecalcularDeuda(id);
        await _repoCaja.RegistrarIngreso(total);
        return ResultadoOperacion.Exito(id);
    }
}

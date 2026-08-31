using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class CuentaCorrienteClienteLogica : ICuentaCorrienteClienteLogica
{
    private readonly ICuentaCorrienteClienteRepositorio _repo;
    public CuentaCorrienteClienteLogica(ICuentaCorrienteClienteRepositorio repo) => _repo = repo;

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
            CorreoElectronico = dto.CorreoElectronico ?? string.Empty,
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
        cuenta.CorreoElectronico = dto.CorreoElectronico ?? string.Empty;
        cuenta.MontoAdeudado = dto.MontoAdeudado;
        cuenta.Estado = EstadoSegunDeuda(dto.MontoAdeudado);
        await _repo.Actualizar(cuenta);
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

    // Cancela total o parcialmente la deuda del cliente.
    public async Task<ResultadoOperacion> RegistrarPago(int id, PagoCuentaCorrienteDto dto)
    {
        var cuenta = await _repo.ObtenerPorId(id);
        if (cuenta is null) return ResultadoOperacion.NoEncontrado("cuenta corriente no encontrada");

        if (cuenta.MontoAdeudado <= 0)
            return ResultadoOperacion.Invalido("la cuenta no tiene deuda pendiente");

        if (dto.Monto > cuenta.MontoAdeudado)
            return ResultadoOperacion.Invalido(
                $"el pago supera la deuda actual de {cuenta.MontoAdeudado:0.##}");

        await _repo.AjustarDeuda(id, -dto.Monto);
        return ResultadoOperacion.Exito(id);
    }
}

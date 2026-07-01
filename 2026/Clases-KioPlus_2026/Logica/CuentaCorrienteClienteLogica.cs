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
            MontoAdeudado = 0,
            Estado = CuentaCorrienteCliente.EstadoDeuda.AlDia
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
        await _repo.Actualizar(cuenta);
        return true;
    }

    public async Task<bool> Eliminar(int id)
    {
        var cuenta = await _repo.ObtenerPorId(id);
        if (cuenta is null) return false;

        await _repo.Eliminar(cuenta);
        return true;
    }
}

using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;


public class ProveedorLogica : IProveedorLogica
{
    private readonly IProveedorRepositorio _repo;
    public ProveedorLogica(IProveedorRepositorio repo) => _repo = repo;

    private static ProveedorDto AMapa(Proveedor p) =>
        new(p.Id, p.NombreRazonSocial, p.Telefono, p.Direccion, p.CorreoElectronico, p.Observaciones);

    public async Task<IEnumerable<ProveedorDto>> ObtenerTodos(string? busqueda)
    {
        var proveedores = await _repo.ObtenerTodos(busqueda);
        return proveedores.Select(AMapa);
    }

    public async Task<ProveedorDto?> ObtenerPorId(int id)
    {
        var p = await _repo.ObtenerPorId(id);
        return p is null ? null : AMapa(p);
    }

    public async Task<int> Crear(ProveedorCreateDto dto)
    {
        var proveedor = new Proveedor
        {
            NombreRazonSocial = dto.NombreRazonSocial,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            CorreoElectronico = dto.CorreoElectronico,
            Observaciones = dto.Observaciones
        };
        await _repo.Agregar(proveedor);
        return proveedor.Id;
    }

    public async Task<bool> Actualizar(int id, ProveedorCreateDto dto)
    {
        var proveedor = await _repo.ObtenerPorId(id);
        if (proveedor is null) return false;

        proveedor.NombreRazonSocial = dto.NombreRazonSocial;
        proveedor.Telefono = dto.Telefono;
        proveedor.Direccion = dto.Direccion;
        proveedor.CorreoElectronico = dto.CorreoElectronico;
        proveedor.Observaciones = dto.Observaciones;
        await _repo.Actualizar(proveedor);
        return true;
    }

    public async Task<bool> Eliminar(int id)
    {
        var proveedor = await _repo.ObtenerPorId(id);
        if (proveedor is null) return false;

        await _repo.Eliminar(proveedor);
        return true;
    }
}

using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;
using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;

public interface ICategoriaLogica
{
    Task<IEnumerable<CategoriaDto>> ObtenerTodas();
    Task<CategoriaDto?> ObtenerPorId(int id);
    Task<int> Crear(CategoriaCreateDto dto);
    Task<bool> Actualizar(int id, CategoriaCreateDto dto);
    Task<bool> Eliminar(int id);
}

public class CategoriaLogica : ICategoriaLogica
{
    private readonly ICategoriaRepositorio _repo;
    public CategoriaLogica(ICategoriaRepositorio repo) => _repo = repo;

    public async Task<IEnumerable<CategoriaDto>> ObtenerTodas()
    {
        var categorias = await _repo.ObtenerTodas();
        return categorias.Select(c => new CategoriaDto(c.Id, c.Nombre, c.Descripcion));
    }

    public async Task<CategoriaDto?> ObtenerPorId(int id)
    {
        var c = await _repo.ObtenerPorId(id);
        return c is null ? null : new CategoriaDto(c.Id, c.Nombre, c.Descripcion);
    }

    public async Task<int> Crear(CategoriaCreateDto dto)
    {
        var categoria = new Categoria { Nombre = dto.Nombre, Descripcion = dto.Descripcion };
        await _repo.Agregar(categoria);
        return categoria.Id;
    }

    public async Task<bool> Actualizar(int id, CategoriaCreateDto dto)
    {
        var categoria = await _repo.ObtenerPorId(id);
        if (categoria is null) return false;

        categoria.Nombre = dto.Nombre;
        categoria.Descripcion = dto.Descripcion;
        await _repo.Actualizar(categoria);
        return true;
    }

    public async Task<bool> Eliminar(int id)
    {
        var categoria = await _repo.ObtenerPorId(id);
        if (categoria is null) return false;

        await _repo.Eliminar(categoria);
        return true;
    }
}

using KioPlusFront.Models.Api;

namespace KioPlusFront.Services;

public interface ICategoriaService
{
    Task<IReadOnlyList<CategoriaDto>> ObtenerTodasAsync();
    Task<ApiResultado<CategoriaDto>> ObtenerPorIdAsync(int id);
    Task<ApiResultado> CrearAsync(CategoriaCreateDto dto);
    Task<ApiResultado> ActualizarAsync(int id, CategoriaCreateDto dto);
    Task<ApiResultado> EliminarAsync(int id);
}

public class CategoriaService : ICategoriaService
{
    private readonly ApiClient _api;
    public CategoriaService(ApiClient api) => _api = api;

    public Task<IReadOnlyList<CategoriaDto>> ObtenerTodasAsync() =>
        _api.ObtenerListaAsync<CategoriaDto>("/categorias");

    public Task<ApiResultado<CategoriaDto>> ObtenerPorIdAsync(int id) =>
        _api.ObtenerAsync<CategoriaDto>($"/categorias/{id}");

    public Task<ApiResultado> CrearAsync(CategoriaCreateDto dto) =>
        _api.PostAsync("/categorias", dto);

    public Task<ApiResultado> ActualizarAsync(int id, CategoriaCreateDto dto) =>
        _api.PutAsync($"/categorias/{id}", dto);

    public Task<ApiResultado> EliminarAsync(int id) =>
        _api.DeleteAsync($"/categorias/{id}");
}

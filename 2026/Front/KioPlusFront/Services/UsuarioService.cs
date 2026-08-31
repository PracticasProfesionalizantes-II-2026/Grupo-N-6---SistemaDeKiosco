using KioPlusFront.Models.Api;

namespace KioPlusFront.Services;

public interface IUsuarioService
{
    Task<ApiResultado<LoginResultadoDto>> LoginAsync(LoginDto dto);
    Task<IReadOnlyList<UsuarioDto>> ObtenerTodosAsync();
    Task<ApiResultado<UsuarioDto>> ObtenerPorIdAsync(int id);
    Task<ApiResultado> CrearAsync(UsuarioCreateDto dto);
    Task<ApiResultado> ActualizarAsync(int id, UsuarioCreateDto dto);
    Task<ApiResultado> CambiarEstadoAsync(int id, bool estado);
    Task<ApiResultado> EliminarAsync(int id);
}

public class UsuarioService : IUsuarioService
{
    private readonly ApiClient _api;
    public UsuarioService(ApiClient api) => _api = api;

    public Task<ApiResultado<LoginResultadoDto>> LoginAsync(LoginDto dto) =>
        _api.PostAsync<LoginDto, LoginResultadoDto>("/usuarios/login", dto);

    public Task<IReadOnlyList<UsuarioDto>> ObtenerTodosAsync() =>
        _api.ObtenerListaAsync<UsuarioDto>("/usuarios");

    public Task<ApiResultado<UsuarioDto>> ObtenerPorIdAsync(int id) =>
        _api.ObtenerAsync<UsuarioDto>($"/usuarios/{id}");

    public Task<ApiResultado> CrearAsync(UsuarioCreateDto dto) =>
        _api.PostAsync("/usuarios", dto);

    public Task<ApiResultado> ActualizarAsync(int id, UsuarioCreateDto dto) =>
        _api.PutAsync($"/usuarios/{id}", dto);

    public Task<ApiResultado> CambiarEstadoAsync(int id, bool estado) =>
        _api.PatchAsync($"/usuarios/{id}/estado", new CambiarEstadoUsuarioDto(estado));

    public Task<ApiResultado> EliminarAsync(int id) =>
        _api.DeleteAsync($"/usuarios/{id}");
}

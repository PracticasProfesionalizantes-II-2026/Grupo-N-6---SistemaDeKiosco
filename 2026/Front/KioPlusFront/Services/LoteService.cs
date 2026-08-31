using KioPlusFront.Models.Api;

namespace KioPlusFront.Services;

public interface ILoteService
{
    Task<IReadOnlyList<LoteDto>> ObtenerPorProductoAsync(int idProducto);
    Task<ApiResultado> CrearAsync(int idProducto, LoteCreateDto dto);
    Task<ApiResultado> ActualizarAsync(int idLote, LoteCreateDto dto);
    Task<ApiResultado> EliminarAsync(int idLote);
}

public class LoteService : ILoteService
{
    private readonly ApiClient _api;
    public LoteService(ApiClient api) => _api = api;

    public Task<IReadOnlyList<LoteDto>> ObtenerPorProductoAsync(int idProducto) =>
        _api.ObtenerListaAsync<LoteDto>($"/productos/{idProducto}/lotes");

    public Task<ApiResultado> CrearAsync(int idProducto, LoteCreateDto dto) =>
        _api.PostAsync($"/productos/{idProducto}/lotes", dto);

    public Task<ApiResultado> ActualizarAsync(int idLote, LoteCreateDto dto) =>
        _api.PutAsync($"/lotes/{idLote}", dto);

    public Task<ApiResultado> EliminarAsync(int idLote) =>
        _api.DeleteAsync($"/lotes/{idLote}");
}

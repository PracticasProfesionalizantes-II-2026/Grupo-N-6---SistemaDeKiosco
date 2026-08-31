using KioPlusFront.Models.Api;

namespace KioPlusFront.Services;

public record CompraCreadaDto(int IdCompraProveedor, int IdProveedor);

public interface ICompraService
{
    Task<IReadOnlyList<CompraListadoDto>> ObtenerTodasAsync(
        DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idProveedor = null);
    Task<ApiResultado<CompraDto>> ObtenerPorIdAsync(int id);
    Task<ApiResultado<CompraCreadaDto>> CrearAsync(CompraCreateDto dto);
    Task<ApiResultado> EliminarAsync(int id);

    Task<IReadOnlyList<DetalleCompraListadoDto>> ObtenerDetallesAsync(int idCompra);
    Task<ApiResultado> AgregarDetalleAsync(int idCompra, DetalleCompraCreateDto dto);
    Task<ApiResultado> EliminarDetalleAsync(int idDetalle);
}

public class CompraService : ICompraService
{
    private readonly ApiClient _api;
    public CompraService(ApiClient api) => _api = api;

    public Task<IReadOnlyList<CompraListadoDto>> ObtenerTodasAsync(
        DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idProveedor = null)
    {
        var filtros = new List<string>();
        if (fechaDesde.HasValue) filtros.Add($"fechaDesde={fechaDesde.Value:yyyy-MM-dd}");
        if (fechaHasta.HasValue) filtros.Add($"fechaHasta={fechaHasta.Value:yyyy-MM-dd}");
        if (idProveedor.HasValue) filtros.Add($"idProveedor={idProveedor.Value}");

        var query = filtros.Count > 0 ? "?" + string.Join("&", filtros) : string.Empty;
        return _api.ObtenerListaAsync<CompraListadoDto>($"/compras{query}");
    }

    public Task<ApiResultado<CompraDto>> ObtenerPorIdAsync(int id) =>
        _api.ObtenerAsync<CompraDto>($"/compras/{id}");

    public Task<ApiResultado<CompraCreadaDto>> CrearAsync(CompraCreateDto dto) =>
        _api.PostAsync<CompraCreateDto, CompraCreadaDto>("/compras", dto);

    public Task<ApiResultado> EliminarAsync(int id) =>
        _api.DeleteAsync($"/compras/{id}");

    public Task<IReadOnlyList<DetalleCompraListadoDto>> ObtenerDetallesAsync(int idCompra) =>
        _api.ObtenerListaAsync<DetalleCompraListadoDto>($"/compras/{idCompra}/detalles");

    public Task<ApiResultado> AgregarDetalleAsync(int idCompra, DetalleCompraCreateDto dto) =>
        _api.PostAsync($"/compras/{idCompra}/detalles", dto);

    public Task<ApiResultado> EliminarDetalleAsync(int idDetalle) =>
        _api.DeleteAsync($"/detallecompras/{idDetalle}");
}

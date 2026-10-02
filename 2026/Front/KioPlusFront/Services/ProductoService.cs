using KioPlusFront.Models.Api;

namespace KioPlusFront.Services;

// Respuesta de creación de la API (solo trae el id generado)
public record ProductoCreadoDto(int IdProducto);

public interface IProductoService
{
    Task<IReadOnlyList<ProductoDto>> ObtenerTodosAsync(string? nombre = null, int? idCategoria = null, string? marca = null);
    Task<ApiResultado<ProductoDetalleDto>> ObtenerPorIdAsync(int id);
    Task<IReadOnlyList<StockCriticoDto>> ObtenerStockCriticoAsync();
    Task<IReadOnlyList<ProximoVencimientoDto>> ObtenerProximosAVencerAsync();
    Task<IReadOnlyList<MasVendidoCantidadDto>> MasVendidosPorCantidadAsync(DateTime? desde, DateTime? hasta, int limite);
    Task<IReadOnlyList<MasVendidoMontoDto>> MasVendidosPorMontoAsync(DateTime? desde, DateTime? hasta, int limite);
    Task<ApiResultado<ProductoCreadoDto>> CrearAsync(ProductoCreateDto dto);
    Task<ApiResultado> ActualizarAsync(int id, ProductoCreateDto dto);
    Task<ApiResultado> EliminarAsync(int id);
}

public class ProductoService : IProductoService
{
    private readonly ApiClient _api;
    public ProductoService(ApiClient api) => _api = api;

    public Task<IReadOnlyList<ProductoDto>> ObtenerTodosAsync(string? nombre = null, int? idCategoria = null, string? marca = null)
    {
        var filtros = new List<string>();
        if (!string.IsNullOrWhiteSpace(nombre)) filtros.Add($"nombre={Uri.EscapeDataString(nombre)}");
        if (idCategoria.HasValue) filtros.Add($"idCategoria={idCategoria.Value}");
        if (!string.IsNullOrWhiteSpace(marca)) filtros.Add($"marca={Uri.EscapeDataString(marca)}");

        var query = filtros.Count > 0 ? "?" + string.Join("&", filtros) : string.Empty;
        return _api.ObtenerListaAsync<ProductoDto>($"/productos{query}");
    }

    public Task<ApiResultado<ProductoDetalleDto>> ObtenerPorIdAsync(int id) =>
        _api.ObtenerAsync<ProductoDetalleDto>($"/productos/{id}");

    public Task<IReadOnlyList<StockCriticoDto>> ObtenerStockCriticoAsync() =>
        _api.ObtenerListaAsync<StockCriticoDto>("/productos?stockCritico=true");

    public Task<IReadOnlyList<ProximoVencimientoDto>> ObtenerProximosAVencerAsync() =>
        _api.ObtenerListaAsync<ProximoVencimientoDto>("/productos?proximoVencimiento=true");

    public Task<IReadOnlyList<MasVendidoCantidadDto>> MasVendidosPorCantidadAsync(DateTime? desde, DateTime? hasta, int limite) =>
        _api.ObtenerListaAsync<MasVendidoCantidadDto>(RutaMasVendidos(desde, hasta, "cantidad", limite));

    public Task<IReadOnlyList<MasVendidoMontoDto>> MasVendidosPorMontoAsync(DateTime? desde, DateTime? hasta, int limite) =>
        _api.ObtenerListaAsync<MasVendidoMontoDto>(RutaMasVendidos(desde, hasta, "monto", limite));

    // Las fechas que no vienen se omiten de la ruta: el ranking sale histórico
    private static string RutaMasVendidos(DateTime? desde, DateTime? hasta, string criterio, int limite)
    {
        var ruta = $"/productos/mas-vendidos?criterio={criterio}&limite={limite}";
        if (desde is not null) ruta += $"&fechaDesde={desde:yyyy-MM-dd}";
        if (hasta is not null) ruta += $"&fechaHasta={hasta:yyyy-MM-dd}";
        return ruta;
    }

    public Task<ApiResultado<ProductoCreadoDto>> CrearAsync(ProductoCreateDto dto) =>
        _api.PostAsync<ProductoCreateDto, ProductoCreadoDto>("/productos", dto);

    public Task<ApiResultado> ActualizarAsync(int id, ProductoCreateDto dto) =>
        _api.PutAsync($"/productos/{id}", dto);

    public Task<ApiResultado> EliminarAsync(int id) =>
        _api.DeleteAsync($"/productos/{id}");
}

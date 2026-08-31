using KioPlusFront.Models.Api;

namespace KioPlusFront.Services;

public interface IProveedorService
{
    Task<IReadOnlyList<ProveedorDto>> ObtenerTodosAsync(string? busqueda = null);
    Task<ApiResultado<ProveedorDto>> ObtenerPorIdAsync(int id);
    Task<ApiResultado> CrearAsync(ProveedorCreateDto dto);
    Task<ApiResultado> ActualizarAsync(int id, ProveedorCreateDto dto);
    Task<ApiResultado> EliminarAsync(int id);

    Task<IReadOnlyList<ProductoProveedorDto>> ObtenerProveedoresDeProductoAsync(int idProducto);
    Task<ApiResultado> AsociarProductoAsync(int idProducto, ProductoProveedorCreateDto dto);
    Task<ApiResultado> ActualizarPrecioAsync(int idProducto, int idAsociacion, double precioCompra);
    Task<ApiResultado> DesasociarProductoAsync(int idProducto, int idAsociacion);
}

public class ProveedorService : IProveedorService
{
    private readonly ApiClient _api;
    public ProveedorService(ApiClient api) => _api = api;

    public Task<IReadOnlyList<ProveedorDto>> ObtenerTodosAsync(string? busqueda = null)
    {
        var query = string.IsNullOrWhiteSpace(busqueda)
            ? string.Empty
            : $"?busqueda={Uri.EscapeDataString(busqueda)}";
        return _api.ObtenerListaAsync<ProveedorDto>($"/proveedores{query}");
    }

    public Task<ApiResultado<ProveedorDto>> ObtenerPorIdAsync(int id) =>
        _api.ObtenerAsync<ProveedorDto>($"/proveedores/{id}");

    public Task<ApiResultado> CrearAsync(ProveedorCreateDto dto) =>
        _api.PostAsync("/proveedores", dto);

    public Task<ApiResultado> ActualizarAsync(int id, ProveedorCreateDto dto) =>
        _api.PutAsync($"/proveedores/{id}", dto);

    public Task<ApiResultado> EliminarAsync(int id) =>
        _api.DeleteAsync($"/proveedores/{id}");

    public Task<IReadOnlyList<ProductoProveedorDto>> ObtenerProveedoresDeProductoAsync(int idProducto) =>
        _api.ObtenerListaAsync<ProductoProveedorDto>($"/productos/{idProducto}/proveedores");

    public Task<ApiResultado> AsociarProductoAsync(int idProducto, ProductoProveedorCreateDto dto) =>
        _api.PostAsync($"/productos/{idProducto}/proveedores", dto);

    public Task<ApiResultado> ActualizarPrecioAsync(int idProducto, int idAsociacion, double precioCompra) =>
        _api.PutAsync($"/productos/{idProducto}/proveedores/{idAsociacion}",
            new ProductoProveedorUpdateDto(precioCompra));

    public Task<ApiResultado> DesasociarProductoAsync(int idProducto, int idAsociacion) =>
        _api.DeleteAsync($"/productos/{idProducto}/proveedores/{idAsociacion}");
}

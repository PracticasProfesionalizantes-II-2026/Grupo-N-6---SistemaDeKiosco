using KioPlusFront.Models.Api;

namespace KioPlusFront.Services;

// Respuestas de creación de la API (solo traen el id generado)
public record VentaCreadaDto(int IdVenta, int IdUsuario);
public record DetalleVentaCreadoDto(int IdDetalle);

public interface IVentaService
{
    Task<IReadOnlyList<VentaListadoDto>> ObtenerTodasAsync(
        DateTime? fechaDesde = null, DateTime? fechaHasta = null,
        int? idUsuario = null, int? idCliente = null,
        double? importeMayorA = null, double? importeMenorA = null);
    Task<ApiResultado<VentaDto>> ObtenerPorIdAsync(int id);
    Task<ApiResultado<VentaCreadaDto>> CrearAsync(VentaCreateDto dto);
    Task<ApiResultado> FinalizarAsync(int idVenta);
    Task<ApiResultado> EliminarAsync(int idVenta);

    Task<IReadOnlyList<DetalleVentaListadoDto>> ObtenerDetallesAsync(int idVenta);
    Task<ApiResultado> AgregarDetalleAsync(int idVenta, DetalleVentaCreateDto dto);
    Task<ApiResultado> EliminarDetalleAsync(int idVenta, int idDetalle);
}

public class VentaService : IVentaService
{
    private readonly ApiClient _api;
    public VentaService(ApiClient api) => _api = api;

    public Task<IReadOnlyList<VentaListadoDto>> ObtenerTodasAsync(
        DateTime? fechaDesde = null, DateTime? fechaHasta = null,
        int? idUsuario = null, int? idCliente = null,
        double? importeMayorA = null, double? importeMenorA = null)
    {
        var filtros = new List<string>();
        if (fechaDesde.HasValue) filtros.Add($"fechaDesde={fechaDesde.Value:yyyy-MM-dd}");
        if (fechaHasta.HasValue) filtros.Add($"fechaHasta={fechaHasta.Value:yyyy-MM-dd}");
        if (idUsuario.HasValue) filtros.Add($"idUsuario={idUsuario.Value}");
        if (idCliente.HasValue) filtros.Add($"idCliente={idCliente.Value}");
        if (importeMayorA.HasValue) filtros.Add($"importeMayorA={importeMayorA.Value}");
        if (importeMenorA.HasValue) filtros.Add($"importeMenorA={importeMenorA.Value}");

        var query = filtros.Count > 0 ? "?" + string.Join("&", filtros) : string.Empty;
        return _api.ObtenerListaAsync<VentaListadoDto>($"/ventas{query}");
    }

    public Task<ApiResultado<VentaDto>> ObtenerPorIdAsync(int id) =>
        _api.ObtenerAsync<VentaDto>($"/ventas/{id}");

    public Task<ApiResultado<VentaCreadaDto>> CrearAsync(VentaCreateDto dto) =>
        _api.PostAsync<VentaCreateDto, VentaCreadaDto>("/ventas", dto);

    public Task<ApiResultado> FinalizarAsync(int idVenta) =>
        _api.PostAsync<object?>($"/ventas/{idVenta}/finalizar", null);

    public Task<ApiResultado> EliminarAsync(int idVenta) =>
        _api.DeleteAsync($"/ventas/{idVenta}");

    public Task<IReadOnlyList<DetalleVentaListadoDto>> ObtenerDetallesAsync(int idVenta) =>
        _api.ObtenerListaAsync<DetalleVentaListadoDto>($"/ventas/{idVenta}/detalles");

    public Task<ApiResultado> AgregarDetalleAsync(int idVenta, DetalleVentaCreateDto dto) =>
        _api.PostAsync($"/ventas/{idVenta}/detalles", dto);

    public Task<ApiResultado> EliminarDetalleAsync(int idVenta, int idDetalle) =>
        _api.DeleteAsync($"/ventas/{idVenta}/detalles/{idDetalle}");
}

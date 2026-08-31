using KioPlusFront.Models.Api;

namespace KioPlusFront.Services;

public interface ICuentaCorrienteService
{
    Task<IReadOnlyList<CuentaCorrienteClienteDto>> ObtenerTodasAsync(
        string? nombre = null, string? apellido = null, int? dni = null,
        EstadoDeuda? estado = null, double? montoMin = null, double? montoMax = null);
    Task<ApiResultado<CuentaCorrienteClienteDto>> ObtenerPorIdAsync(int id);
    Task<ApiResultado> CrearAsync(CuentaCorrienteClienteCreateDto dto);
    Task<ApiResultado> ActualizarAsync(int id, CuentaCorrienteClienteCreateDto dto);
    Task<ApiResultado> RegistrarPagoAsync(int id, double monto);
    Task<ApiResultado> EliminarAsync(int id);
}

public class CuentaCorrienteService : ICuentaCorrienteService
{
    // Coincide con CuentaCorrienteCliente.IdConsumidorFinal del back
    public const int IdConsumidorFinal = 1;

    private readonly ApiClient _api;
    public CuentaCorrienteService(ApiClient api) => _api = api;

    public Task<IReadOnlyList<CuentaCorrienteClienteDto>> ObtenerTodasAsync(
        string? nombre = null, string? apellido = null, int? dni = null,
        EstadoDeuda? estado = null, double? montoMin = null, double? montoMax = null)
    {
        var filtros = new List<string>();
        if (!string.IsNullOrWhiteSpace(nombre)) filtros.Add($"nombre={Uri.EscapeDataString(nombre)}");
        if (!string.IsNullOrWhiteSpace(apellido)) filtros.Add($"apellido={Uri.EscapeDataString(apellido)}");
        if (dni.HasValue) filtros.Add($"dni={dni.Value}");
        if (estado.HasValue) filtros.Add($"estado={estado.Value}");
        if (montoMin.HasValue) filtros.Add($"montoAdeudadoMin={montoMin.Value}");
        if (montoMax.HasValue) filtros.Add($"montoAdeudadoMax={montoMax.Value}");

        var query = filtros.Count > 0 ? "?" + string.Join("&", filtros) : string.Empty;
        return _api.ObtenerListaAsync<CuentaCorrienteClienteDto>($"/cuentas-corrientes-clientes{query}");
    }

    public Task<ApiResultado<CuentaCorrienteClienteDto>> ObtenerPorIdAsync(int id) =>
        _api.ObtenerAsync<CuentaCorrienteClienteDto>($"/cuentas-corrientes-clientes/{id}");

    public Task<ApiResultado> CrearAsync(CuentaCorrienteClienteCreateDto dto) =>
        _api.PostAsync("/cuentas-corrientes-clientes", dto);

    public Task<ApiResultado> ActualizarAsync(int id, CuentaCorrienteClienteCreateDto dto) =>
        _api.PutAsync($"/cuentas-corrientes-clientes/{id}", dto);

    public Task<ApiResultado> RegistrarPagoAsync(int id, double monto) =>
        _api.PostAsync($"/cuentas-corrientes-clientes/{id}/pagos", new PagoCuentaCorrienteDto(monto));

    public Task<ApiResultado> EliminarAsync(int id) =>
        _api.DeleteAsync($"/cuentas-corrientes-clientes/{id}");
}

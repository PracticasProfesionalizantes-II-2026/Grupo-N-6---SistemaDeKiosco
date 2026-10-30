using KioPlusFront.Models.Api;

namespace KioPlusFront.Services;

public interface ILectorFacturaService
{
    Task<ApiResultado<FacturaInterpretadaDto>> InterpretarAsync(IFormFile pdf);
}

// Usa su propio HttpClient (registrado en Program.cs) porque leer una factura con
// IA tarda bastante más que los 15 segundos del cliente general de la API.
public class LectorFacturaService : ILectorFacturaService
{
    private readonly ApiClient _api;

    public LectorFacturaService(HttpClient http, ILogger<ApiClient> logger) =>
        _api = new ApiClient(http, logger);

    public Task<ApiResultado<FacturaInterpretadaDto>> InterpretarAsync(IFormFile pdf) =>
        _api.PostArchivoAsync<FacturaInterpretadaDto>("/compras/interpretar-factura", pdf);
}

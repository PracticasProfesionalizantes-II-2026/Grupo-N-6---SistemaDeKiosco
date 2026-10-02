namespace KioPlusFront.Services;

public record SaldoCajaDto(double SaldoActual);

public interface ICajaService
{
    Task<double> SaldoActualAsync();
}

// La caja no tiene pantalla propia: el requerimiento la define como lógica
// interna. Solo se consulta su saldo para mostrarlo en el menú.
public class CajaService : ICajaService
{
    private readonly ApiClient _api;
    public CajaService(ApiClient api) => _api = api;

    public async Task<double> SaldoActualAsync()
    {
        var resultado = await _api.ObtenerAsync<SaldoCajaDto>("/caja");
        return resultado.Datos?.SaldoActual ?? 0;
    }
}

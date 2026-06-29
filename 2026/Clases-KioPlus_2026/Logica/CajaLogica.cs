using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;

public interface ICajaLogica
{
    Task<double> SaldoActual();
    Task<double> SaldoAFecha(DateTime fecha);
}

public class CajaLogica : ICajaLogica
{
    private readonly ICajaRepositorio _repo;
    public CajaLogica(ICajaRepositorio repo) => _repo = repo;

    public Task<double> SaldoActual() => _repo.SaldoActual();

    public Task<double> SaldoAFecha(DateTime fecha) => _repo.SaldoAFecha(fecha);
}

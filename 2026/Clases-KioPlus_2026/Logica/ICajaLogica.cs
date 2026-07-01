using Clases_KioPlus.Repositorios;

namespace Clases_KioPlus.Logica;
public interface ICajaLogica
{
    Task<double> SaldoActual();
    Task<double> SaldoAFecha(DateTime fecha);
}
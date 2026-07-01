using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface ICajaRepositorio
{
    Task<double> SaldoActual();
    Task<double> SaldoAFecha(DateTime fecha);
}
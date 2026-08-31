using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface IVentaRepositorio
{
    Task<bool> UsuarioExiste(int idUsuario);
    Task<bool> CuentaExiste(int idCuenta);
    Task<IEnumerable<Venta>> ObtenerTodas();
    // Listado con los nombres de vendedor y cliente ya resueltos
    Task<IEnumerable<(Venta Venta, string Vendedor, string Cliente)>> ObtenerTodasConNombres(
        DateTime? fechaDesde, DateTime? fechaHasta, int? idUsuario, int? idCliente,
        double? importeMayorA, double? importeMenorA);
    Task<Venta?> ObtenerPorId(int id);
    Task<Venta> Agregar(Venta venta);
    Task Actualizar(Venta venta);
    Task Eliminar(Venta venta);
}

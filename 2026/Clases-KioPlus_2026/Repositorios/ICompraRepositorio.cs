using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface ICompraRepositorio
{
    Task<bool> ProveedorExiste(int idProveedor);
    Task<IEnumerable<CompraProveedor>> ObtenerTodas();
    Task<CompraProveedor?> ObtenerPorId(int id);
    Task<CompraProveedor> Agregar(CompraProveedor compra);
    Task Actualizar(CompraProveedor compra);
    Task Eliminar(CompraProveedor compra);
}
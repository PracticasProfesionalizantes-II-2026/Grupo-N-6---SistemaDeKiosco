using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface IProveedorRepositorio
{
    Task<IEnumerable<Proveedor>> ObtenerTodos(string? busqueda);
    Task<Proveedor?> ObtenerPorId(int id);
    Task<Proveedor> Agregar(Proveedor proveedor);
    Task Actualizar(Proveedor proveedor);
    Task Eliminar(Proveedor proveedor);
}
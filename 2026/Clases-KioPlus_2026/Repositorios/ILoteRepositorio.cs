using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface ILoteRepositorio
{
    Task<bool> ProductoExiste(int idProducto);
    Task<IEnumerable<Lote>> ObtenerPorProducto(int idProducto);
    Task<Lote?> ObtenerPorId(int id);
    Task<Lote> Agregar(Lote lote);
    Task Actualizar(Lote lote);
    Task Eliminar(Lote lote);
}
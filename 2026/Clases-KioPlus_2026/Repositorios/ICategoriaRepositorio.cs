using Clases_KioPlus.Data;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Repositorios;

public interface ICategoriaRepositorio
{
    Task<IEnumerable<Categoria>> ObtenerTodas();
    Task<Categoria?> ObtenerPorId(int id);
    Task<Categoria> Agregar(Categoria categoria);
    Task Actualizar(Categoria categoria);
    Task Eliminar(Categoria categoria);
}
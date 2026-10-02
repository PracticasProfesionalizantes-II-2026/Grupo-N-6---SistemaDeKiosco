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

    // Lote generado por un renglón de compra, para poder editarlo o darlo de baja con él
    Task<Lote?> ObtenerPorDetalleCompra(int idDetalleCompra);
    Task EliminarPorDetallesCompra(IEnumerable<int> idsDetalleCompra);

    // Deja StockDisponible del producto igual a la suma de sus lotes
    Task RecalcularStockProducto(int idProducto);

    // Descuenta unidades siguiendo FEFO: primero el lote que vence antes.
    // Devuelve false y no toca nada si el stock no alcanza.
    Task<bool> ConsumirFefo(int idProducto, int cantidad);

    // Repone unidades recorriendo los lotes en el mismo orden FEFO y sin superar
    // la cantidad con la que cada lote ingresó. Revierte exactamente un consumo previo.
    Task DevolverFefo(int idProducto, int cantidad);
}

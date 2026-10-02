using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class Lote
    {
        public int Id { get; set; }
        [Required] public string NroLote { get; set; }

        // Fecha de vencimiento del lote. Es opcional: los productos que no vencen
        // (encendedores, pilas) se cargan sin fecha y se consumen últimos.
        public DateTime? FechaVencimiento { get; set; }

        // Cantidad con la que ingresó el lote. No la mueven las ventas: sirve como
        // tope al devolver unidades cuando se anula o se edita una venta.
        [Required] public int CantidadInicial { get; set; }

        // Cantidad que queda disponible en el lote
        [Required] public int Cantidad { get; set; }

        // Relación con el producto al que pertenece este lote
        [Required] public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        // Renglón de compra que originó este lote. Queda en null si el lote
        // se cargó a mano desde la pantalla de stock. Se resuelve por lógica y no
        // como clave foránea para no encadenar borrados en cascada sobre Producto.
        public int? DetalleCompraId { get; set; }
    }
}
                                                     
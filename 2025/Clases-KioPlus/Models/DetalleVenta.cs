using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }

        // Relación con la venta a la que pertenece este detalle
        [Required] public int VentaId { get; set; }
        public Venta Venta { get; set; }

        // Relación con el producto vendido
        [Required] public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        // Cantidad de unidades vendidas del producto
        [Required] public int Cantidad { get; set; }

        // Precio unitario del producto al momento de la venta
        [Required] public double PrecioUnitario { get; set; }

        // Subtotal para este detalle (Cantidad * PrecioUnitario)
        [Required] public double Subtotal { get; set; }
    }
}

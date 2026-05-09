using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class DetalleCompra
    {
        public int Id { get; set; }

        [Required]
        public int CompraProveedorId { get; set; }
        public CompraProveedor CompraProveedor { get; set; }

        // Producto comprado
        [Required]
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        // Cantidad comprada
        [Required]
        public int Cantidad { get; set; }

        // Precio al momento de la compra
        [Required]
        public double PrecioCompra { get; set; }

        // Subtotal del detalle
        [Required]
        public double Subtotal { get; set; }
    }
}
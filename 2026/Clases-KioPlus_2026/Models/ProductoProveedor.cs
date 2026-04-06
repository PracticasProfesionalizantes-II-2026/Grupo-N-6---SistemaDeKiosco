using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class ProductoProveedor
    {
        public int Id { get; set; }

        // Relación con el producto correspondiente
        [Required] public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        // Relación con el proveedor que ofrece el producto
        [Required] public int ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; }

        // Precio de compra del producto ofrecido por este proveedor
        [Required] public double PrecioCompra { get; set; }
    }
}

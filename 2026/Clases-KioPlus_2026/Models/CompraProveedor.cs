using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class CompraProveedor
    {
        public int Id { get; set; }

        // Relación con el proveedor al que se le realizo la compra
        public int ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; }

        [Required] public DateTime FechaHora { get; set; }

        [Required] public double MontoTotal { get; set; }


        // Relación uno a muchos con DetalleCompra
        public List<DetalleCompra> DetallesCompra { get; set; }
    }
}
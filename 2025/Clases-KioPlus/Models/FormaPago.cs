using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class FormaPago
    {
        public int Id { get; set; }

        // Descripción del método de pago (efectivo, transferencia)
        [Required] public string Descripcion { get; set; }

        // Relación uno a muchos con ventas
        public List<Venta> Ventas { get; set; }
    }
}

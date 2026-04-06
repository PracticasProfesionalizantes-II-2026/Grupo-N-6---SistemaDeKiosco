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

        // Fecha de vencimiento del lote
        [Required] public DateTime FechaVencimiento { get; set; }

        // Cantidad de productos del lote
        [Required] public int Cantidad { get; set; }

        // Relación con el producto al que pertenece este lote
        [Required] public int ProductoId { get; set; }
        public Producto Producto { get; set; }
    }
}
                                                     
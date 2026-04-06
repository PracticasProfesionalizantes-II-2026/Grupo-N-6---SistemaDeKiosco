using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        // Nombre de la categoría del producto (por ejemplo: bebidas, limpieza, etc.)
        [Required] public string Nombre { get; set; }

        // Descripción o detalle adicional de la categoría
        [Required] public string Descripcion { get; set; }

        // Relación uno a muchos con productos
        public List<Producto> Productos { get; set; }
    }
}

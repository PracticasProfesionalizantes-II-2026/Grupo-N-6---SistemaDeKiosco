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

        // Descripción o detalle adicional de la categoría. Es opcional:
        // el nombre ya alcanza para clasificar el producto.
        public string? Descripcion { get; set; }

        // Baja lógica: al eliminar queda en false y se conserva el historial
        public bool Activo { get; set; } = true;

        // Relación uno a muchos con productos
        public List<Producto> Productos { get; set; }
    }
}

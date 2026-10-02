using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class Proveedor
    {
        public int Id { get; set; }
        [Required] public string  NombreRazonSocial { get; set; }
        [Required] public string Telefono { get; set; }
        [Required] public string Direccion { get; set; }

        // Opcional: hay proveedores que solo dejan un teléfono
        public string? CorreoElectronico { get; set; }

        // Opcional: notas sueltas sobre el proveedor
        public string? Observaciones { get; set; }

        // Relación uno a muchos con los productos que provee
        public List<ProductoProveedor> ProductoProveedores { get; set; }
    }
}

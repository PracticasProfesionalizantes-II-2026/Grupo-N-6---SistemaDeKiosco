using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class CuentaCorrienteCliente
    {
        public int Id { get; set; }
        [Required] public string Nombre { get; set; }
        [Required] public string Apellido { get; set; }
        [Required] public int Dni { get; set; }
        [Required] public string Telefono { get; set; }
        [Required] public string Direccion { get; set; }
        public string Email { get; set; }

        // Monto total que el cliente adeuda
        [Required] public double MontoAdeudado { get; set; }

        // Estado actual de la deuda del cliente (Moroso o Al Día)
        [Required] public EstadoDeuda Estado { get; set; }
        public enum EstadoDeuda
        {
            Moroso, // Cliente con deuda pendiente
            AlDia   // Cliente sin deudas
        }
       
        // Relación uno a muchos con las ventas registradas en su cuenta corriente
        public List<Venta> Ventas { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class Venta
    {
        public int Id { get; set; }
        [Required] public DateTime FechaHora { get; set; }

        // Relación con el usuario que realizó la venta
        [Required] public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        [Required] public double MontoTotal { get; set; }

        // Relación con la cuenta corriente del cliente (si aplica)
        [Required] public int CuentaCorrienteClienteId { get; set; }
        public CuentaCorrienteCliente CuentaCorrienteCliente { get; set; }

        // Relación con la forma de pago utilizada en la venta
        [Required] public FormaPago FormaPago { get; set; }
        public DateTime FechaPago { get; set; }

        // Estado actual de la venta (Pagado / NoPagado)
        [Required] public EstadoVenta Estado { get; set; }
        public enum EstadoVenta
        {
            Pagado,  // venta pagada
            NoPagado // venta no pagada
        }

        public enum FormaPago
        {
            CuentaCorriente, // pago a través de la cuenta corriente del cliente
            PagadoAlMomento  // pago en efectivo
        }

        // Relación uno a muchos con DetalleVenta
        public List<DetalleVenta> DetallesVenta { get; set; }
    }
}

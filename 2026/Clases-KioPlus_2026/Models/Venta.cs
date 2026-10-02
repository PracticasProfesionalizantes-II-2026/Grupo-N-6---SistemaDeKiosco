using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [Required] public FormaDePago FormaPago { get; set; }

        // Fecha en la que se cobró la venta. Vacía mientras no se haya pagado.
        public DateTime? FechaPago { get; set; }

        // Estado actual de la venta (Pagado / NoPagado)
        [Required] public EstadoVenta Estado { get; set; }

        // Queda en true al cerrar la venta. Evita que un segundo "Finalizar venta"
        // vuelva a impactar la caja o la cuenta corriente del cliente.
        [Required] public bool Finalizada { get; set; }

        // Cuánto de esta venta ya cobró el kiosco. Permite imputar un pago a cuenta
        // contra las ventas más antiguas y saber cuánto queda pendiente de cada una.
        [Required] public double MontoPagado { get; set; }

        // Lo que todavía se adeuda de esta venta. Se calcula, no se guarda.
        [NotMapped] public double SaldoPendiente => Math.Max(0, MontoTotal - MontoPagado);
        public enum EstadoVenta
        {
            Pagado,  // venta pagada
            NoPagado // venta no pagada
        }

        public enum FormaDePago
        {
            CuentaCorriente, // pago a través de la cuenta corriente del cliente
            PagadoAlMomento  // pago en efectivo
        }

        // Relación uno a muchos con DetalleVenta
        public List<DetalleVenta> DetallesVenta { get; set; }
    }
}

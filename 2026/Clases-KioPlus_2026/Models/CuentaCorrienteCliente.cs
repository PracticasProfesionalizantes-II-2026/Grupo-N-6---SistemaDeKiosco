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
        // Cliente reservado para las ventas sin cuenta corriente ("Consumidor Final").
        // Lo crea DbSeeder al arrancar y no puede eliminarse.
        public const int IdConsumidorFinal = 1;

        public int Id { get; set; }
        [Required] public string Nombre { get; set; }
        [Required] public string Apellido { get; set; }
        [Required] public int Dni { get; set; }
        [Required] public string Telefono { get; set; }
        [Required] public string Direccion { get; set; }
        // Opcional: el cliente puede no tener correo
        public string? CorreoElectronico { get; set; }

        // Monto total que el cliente adeuda. Es un valor derivado: siempre equivale
        // al saldo inicial más lo pendiente de sus ventas en cuenta corriente.
        [Required] public double MontoAdeudado { get; set; }

        // Deuda cargada a mano al dar de alta la cuenta, que no corresponde a
        // ninguna venta registrada en el sistema.
        [Required] public double SaldoInicial { get; set; }

        // Día en que se dio de alta la cuenta (y con ella su saldo inicial). Un
        // pago de la deuda no puede tener una fecha anterior a esta.
        [Required] public DateTime FechaAlta { get; set; }

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

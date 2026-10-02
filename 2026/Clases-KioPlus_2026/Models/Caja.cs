using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class Caja
    {
        public int Id { get; set; }
        //Monto actual de dinero en la caja del sistema
        [Required] public double Monto { get; set; }

        // Entra plata: venta cobrada al momento o pago de una cuenta corriente
        public void RegistrarIngreso(double monto)
        {
            Monto += monto;
        }

        // Sale plata: compra a proveedor
        public void RegistrarEgreso(double monto)
        {
            Monto -= monto;
        }
    }
}

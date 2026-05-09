using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class Caja
    {
        public int Id { get; set; }

        // Monto total actual de dinero en la caja del sistema
        public double Monto { get; set; }

       
        public void RegistrarIngreso(decimal monto)
        {
            MontoActual += monto;
        }

        //manera 1 
        /*
        public void RegistrarEgreso(decimal monto)
        {
            MontoActual -= monto;
        }

        public decimal ObtenerMonto()
        {
            return MontoActual;
        }
        */
        //manera 2
        /*
        public decimal ObtenerMontoCaja()
        {
            decimal totalVentas = ventas.Sum(v => v.MontoTotal);
            decimal totalCompras = compras.Sum(c => c.MontoTotal);

            return totalVentas - totalCompras;
        }
        */
    }
}

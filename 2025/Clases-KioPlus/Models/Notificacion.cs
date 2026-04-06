using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class Notificacion
    {
        public int Id { get; set; }
        // Tipo de notificación (por stock o vencimiento)
        [Required] public TipoNotificacion Tipo { get; set; }

        // Mensaje descriptivo de la notificación
        [Required] public string Mensaje { get; set; }

        // Fecha en la que se generó la notificación
        [Required] public DateTime FechaGeneracion { get; set; }

        // Tipos de notificación posibles
        public enum TipoNotificacion
        {
            StockBajo,          // Notificación por stock bajo   
            ProximoVencimiento  // Notificación por próximo vencimiento    
        }
    }
}

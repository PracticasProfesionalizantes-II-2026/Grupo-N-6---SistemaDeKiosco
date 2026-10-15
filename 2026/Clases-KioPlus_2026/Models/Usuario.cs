using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases_KioPlus.Models
{
    public class Usuario
    {
        public int Id { get; set; }
       [Required] public string NombreApellido { get; set; }
        [Required] public string Telefono { get; set; }

        // Nombre de usuario utilizado para iniciar sesión
        [Required] public string NombreUsuario { get; set; }

        [Required] public string ContraseniaUsuario { get; set; }

        // Tipo de usuario según su rol dentro del sistema
        [Required] public TipoDeUsuario TipoUsuario { get; set; }

        // Estado del usuario (activo/inactivo)
        [Required] public bool Estado { get; set; }

        // Baja lógica: al eliminar queda en false y se conserva el historial
        public bool Activo { get; set; } = true;

        // Relación uno a muchos con ventas realizadas por el usuario
        public List<Venta> Ventas { get; set; }


        public enum TipoDeUsuario
        {
            SuperAdmin,     // Usuario con todos los privilegios
            Administrador,  // Usuario con privilegios administrativos
            Empleado        // Usuario con privilegios limitados
        }

    }
}

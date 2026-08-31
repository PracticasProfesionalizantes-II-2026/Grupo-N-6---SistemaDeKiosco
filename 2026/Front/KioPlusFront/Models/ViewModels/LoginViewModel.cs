using System.ComponentModel.DataAnnotations;

namespace KioPlusFront.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresá tu nombre de usuario")]
    [Display(Name = "Nombre de Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu contraseña")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasenia { get; set; } = string.Empty;
}

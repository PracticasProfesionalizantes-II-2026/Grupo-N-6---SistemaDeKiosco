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

// Pantalla "¿Olvidaste tu contraseña?": pide la nueva clave y su confirmación
public class RecuperarContraseniaViewModel
{
    [Required(ErrorMessage = "Ingresá tu nombre de usuario")]
    [Display(Name = "Nombre de Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá la nueva contraseña")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string NuevaContrasenia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Repetí la nueva contraseña")]
    [DataType(DataType.Password)]
    [Display(Name = "Repetir contraseña")]
    [Compare(nameof(NuevaContrasenia), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmacionContrasenia { get; set; } = string.Empty;
}

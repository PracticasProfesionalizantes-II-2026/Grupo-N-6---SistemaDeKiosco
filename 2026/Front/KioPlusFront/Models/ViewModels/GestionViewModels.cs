using System.ComponentModel.DataAnnotations;
using KioPlusFront.Models.Api;

namespace KioPlusFront.Models.ViewModels;

public class ProveedorFormViewModel
{
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "Ingresá el nombre o razón social")]
    [Display(Name = "Nombre/Razón social")]
    public string NombreRazonSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el teléfono")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá la dirección")]
    [Display(Name = "Dirección")]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el correo electrónico")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    [Display(Name = "Email")]
    public string CorreoElectronico { get; set; } = string.Empty;

    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }
}

public class CuentaCorrienteFormViewModel
{
    public int IdCuentaCorrienteCliente { get; set; }

    [Required(ErrorMessage = "Ingresá el nombre")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el apellido")]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "El DNI no es válido")]
    [Display(Name = "DNI")]
    public int Dni { get; set; }

    [Required(ErrorMessage = "Ingresá el teléfono")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá la dirección")]
    [Display(Name = "Dirección")]
    public string Direccion { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    [Display(Name = "Correo Electrónico (Opcional)")]
    public string? CorreoElectronico { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto adeudado no puede ser negativo")]
    [Display(Name = "Monto adeudado")]
    public double MontoAdeudado { get; set; }
}

public class ListadoCuentasCorrientesViewModel
{
    public IReadOnlyList<CuentaCorrienteClienteDto> Cuentas { get; set; } = Array.Empty<CuentaCorrienteClienteDto>();

    [Display(Name = "Nombre")]
    public string? Nombre { get; set; }

    [Display(Name = "Apellido")]
    public string? Apellido { get; set; }

    [Display(Name = "DNI")]
    public int? Dni { get; set; }

    [Display(Name = "Estado")]
    public EstadoDeuda? Estado { get; set; }

    [Display(Name = "Adeuda desde")]
    public double? MontoMin { get; set; }

    [Display(Name = "Adeuda hasta")]
    public double? MontoMax { get; set; }
}

public class UsuarioFormViewModel
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "Ingresá el nombre y apellido")]
    [Display(Name = "Nombre y Apellido")]
    public string NombreApellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el teléfono")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el nombre de usuario")]
    [Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá la contraseña")]
    [Display(Name = "Contraseña")]
    public string ContraseniaUsuario { get; set; } = string.Empty;

    [Display(Name = "Tipo de usuario")]
    public TipoDeUsuario TipoUsuario { get; set; } = TipoDeUsuario.Empleado;

    [Display(Name = "Activo")]
    public bool Estado { get; set; } = true;
}

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
    [RegularExpression(Formatos.Telefono, ErrorMessage = Formatos.MensajeTelefono)]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá la dirección")]
    [Display(Name = "Dirección")]
    public string Direccion { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    [Display(Name = "Correo Electrónico (Opcional)")]
    public string? CorreoElectronico { get; set; }

    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }
}

public class CuentaCorrienteFormViewModel
{
    public int IdCuentaCorrienteCliente { get; set; }

    [Required(ErrorMessage = "Ingresá el nombre")]
    [RegularExpression(Formatos.SoloLetras, ErrorMessage = "El nombre " + Formatos.MensajeSoloLetras)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el apellido")]
    [RegularExpression(Formatos.SoloLetras, ErrorMessage = "El apellido " + Formatos.MensajeSoloLetras)]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "El DNI no es válido")]
    [Display(Name = "DNI")]
    public int Dni { get; set; }

    [Required(ErrorMessage = "Ingresá el teléfono")]
    [RegularExpression(Formatos.Telefono, ErrorMessage = Formatos.MensajeTelefono)]
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

// Productos que el kiosco le compra a un proveedor. Es la misma asociación que
// se ve desde el producto, recorrida al revés.
public class ProductosDelProveedorViewModel
{
    public int IdProveedor { get; set; }
    public string NombreProveedor { get; set; } = string.Empty;
    public IReadOnlyList<ProductoDelProveedorDto> Productos { get; set; } = Array.Empty<ProductoDelProveedorDto>();

    // Solo los que todavía no están asociados a este proveedor
    public IReadOnlyList<ProductoDto> ProductosDisponibles { get; set; } = Array.Empty<ProductoDto>();

    [Range(1, int.MaxValue, ErrorMessage = "Elegí un producto")]
    [Display(Name = "Producto")]
    public int IdProducto { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    [Display(Name = "Precio de compra")]
    public double PrecioCompra { get; set; }
}

public class ListadoCuentasCorrientesViewModel
{
    public IReadOnlyList<CuentaCorrienteClienteDto> Cuentas { get; set; } = Array.Empty<CuentaCorrienteClienteDto>();

    [Display(Name = "Nombre")]
    public string? Nombre { get; set; }

    [Display(Name = "Apellido")]
    public string? Apellido { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El DNI tiene que ser un número positivo")]
    [Display(Name = "DNI")]
    public int? Dni { get; set; }

    [Display(Name = "Estado")]
    public EstadoDeuda? Estado { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo")]
    [Display(Name = "Adeuda desde")]
    public double? MontoMin { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo")]
    [Display(Name = "Adeuda hasta")]
    public double? MontoMax { get; set; }
}

// Deuda de un cliente: sus ventas en cuenta corriente todavía impagas
public class DeudaClienteViewModel
{
    public int IdCuentaCorrienteCliente { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public double MontoAdeudado { get; set; }
    public IReadOnlyList<VentaAdeudadaDto> Ventas { get; set; } = Array.Empty<VentaAdeudadaDto>();

    // Lo que se debe por fuera de las ventas registradas (saldo cargado al alta)
    public double SaldoSinVentas => Math.Max(0, MontoAdeudado - Ventas.Sum(v => v.SaldoPendiente));
}

public class UsuarioFormViewModel
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "Ingresá el nombre y apellido")]
    [RegularExpression(Formatos.SoloLetras, ErrorMessage = "El nombre y apellido " + Formatos.MensajeSoloLetras)]
    [Display(Name = "Nombre y Apellido")]
    public string NombreApellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el teléfono")]
    [RegularExpression(Formatos.Telefono, ErrorMessage = Formatos.MensajeTelefono)]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el nombre de usuario")]
    [Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    // Al editar se deja vacía para no cambiarla; al crear es obligatoria y eso
     // se valida en el controlador, porque el mismo formulario sirve para los dos.
    [Display(Name = "Contraseña")]
    public string? ContraseniaUsuario { get; set; }

    [Display(Name = "Tipo de usuario")]
    public TipoDeUsuario TipoUsuario { get; set; } = TipoDeUsuario.Empleado;

    [Display(Name = "Activo")]
    public bool Estado { get; set; } = true;
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace KioPlusFront.Models;

// MVC agrega solo un [Required] a todo campo de tipo valor que no sea nulable
// (int, double, DateTime). Ese atributo no está escrito en ningún ViewModel, así
// que no hay dónde ponerle el mensaje: usa el texto por defecto de .NET, en
// inglés ("The Precio de venta field is required.").
//
// Esto importa más de lo que parece: un <input type="number"> con min devuelve
// cadena vacía cuando el valor no cumple, por ejemplo al escribir un negativo.
// El navegador entonces no dispara la regla del rango sino la de obligatorio, y
// el usuario termina viendo el mensaje en inglés justo en el caso del negativo.
//
// Acá se completa el mensaje de los atributos que llegan sin uno propio. Los que
// ya traen ErrorMessage en su ViewModel no se tocan: ese gana siempre.
public class MensajesDeValidacion : IValidationAttributeAdapterProvider
{
    private static readonly Dictionary<Type, string> PorDefecto = new()
    {
        [typeof(RequiredAttribute)] = "Completá {0}",
        [typeof(EmailAddressAttribute)] = "{0} no es un correo válido",
        [typeof(PhoneAttribute)] = "{0} no es un teléfono válido",
        [typeof(RangeAttribute)] = "{0} tiene que estar entre {1} y {2}",
        [typeof(StringLengthAttribute)] = "{0} es demasiado largo",
        [typeof(MinLengthAttribute)] = "{0} es demasiado corto",
        [typeof(MaxLengthAttribute)] = "{0} es demasiado largo",
        [typeof(CompareAttribute)] = "{0} no coincide",
        [typeof(RegularExpressionAttribute)] = "{0} no tiene el formato esperado",
    };

    // El proveedor que trae MVC: una vez puesto el mensaje, resuelve como siempre
    private readonly ValidationAttributeAdapterProvider _original = new();

    public IAttributeAdapter? GetAttributeAdapter(
        ValidationAttribute attribute, IStringLocalizer? stringLocalizer)
    {
        var sinMensajePropio = attribute.ErrorMessage is null
                               && attribute.ErrorMessageResourceName is null;

        if (sinMensajePropio && PorDefecto.TryGetValue(attribute.GetType(), out var mensaje))
            attribute.ErrorMessage = mensaje;

        return _original.GetAttributeAdapter(attribute, stringLocalizer);
    }
}

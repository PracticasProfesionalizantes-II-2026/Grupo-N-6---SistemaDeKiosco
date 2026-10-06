namespace KioPlusFront.Models;

// Formatos que se validan en más de un formulario. Son las mismas expresiones
// que usa la API (Clases_KioPlus.Logica.DTOs.Formatos): si se cambia una, hay
// que cambiar la otra. Se validan también en el navegador, así que tienen que
// ser expresiones válidas en JavaScript.
public static class Formatos
{
    // Solo letras (con tildes y ñ) y espacios, para nombres compuestos
    public const string SoloLetras = @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ ]+$";

    // Dígitos, espacios, guiones y paréntesis, con un + opcional al principio.
    // Admite "+54 9 3442 234134" o "(3442) 15-234134".
    public const string Telefono = @"^\+?[0-9 ()-]{6,25}$";

    public const string MensajeSoloLetras = "solo puede contener letras y espacios";
    public const string MensajeTelefono = "El teléfono solo puede contener números, espacios, guiones, paréntesis y un + al principio";
}

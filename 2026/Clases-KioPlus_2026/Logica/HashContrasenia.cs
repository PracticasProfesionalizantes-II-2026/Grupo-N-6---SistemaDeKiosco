using System.Security.Cryptography;
using System.Text;

namespace Clases_KioPlus.Logica;

// Las contraseñas nunca se guardan tal cual: se guarda su hash SHA-256 en
// hexadecimal (64 caracteres). Para validar un login se hashea lo que escribió
// el usuario y se compara con lo guardado.
public static class HashContrasenia
{
    public static string Calcular(string contrasenia)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(contrasenia));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    // Sirve para detectar contraseñas guardadas antes de usar el hash
    public static bool EsHash(string valor) =>
        valor.Length == 64 && valor.All(Uri.IsHexDigit);
}

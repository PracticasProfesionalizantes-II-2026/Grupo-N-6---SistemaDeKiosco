using System.Text.Json;
using KioPlusFront.Models.Api;

namespace KioPlusFront.Services;

// Datos del usuario logueado que viajan en la sesión del servidor.
public record UsuarioSesion(int IdUsuario, string NombreApellido, string NombreUsuario, TipoDeUsuario TipoUsuario)
{
    public bool EsAdministrador =>
        TipoUsuario is TipoDeUsuario.SuperAdmin or TipoDeUsuario.Administrador;
}

public static class SesionExtensiones
{
    private const string ClaveUsuario = "kioplus.usuario";

    public static void GuardarUsuario(this ISession sesion, UsuarioSesion usuario) =>
        sesion.SetString(ClaveUsuario, JsonSerializer.Serialize(usuario));

    public static UsuarioSesion? ObtenerUsuario(this ISession sesion)
    {
        var json = sesion.GetString(ClaveUsuario);
        return string.IsNullOrEmpty(json)
            ? null
            : JsonSerializer.Deserialize<UsuarioSesion>(json);
    }

    public static void CerrarSesion(this ISession sesion) => sesion.Clear();

    // Guarda cualquier objeto serializable en la sesión (carrito de venta/compra en curso)
    public static void GuardarObjeto<T>(this ISession sesion, string clave, T valor) =>
        sesion.SetString(clave, JsonSerializer.Serialize(valor));

    public static T? ObtenerObjeto<T>(this ISession sesion, string clave)
    {
        var json = sesion.GetString(clave);
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
    }
}

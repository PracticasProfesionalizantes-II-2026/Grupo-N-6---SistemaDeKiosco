namespace KioPlusFront.Services;

// Resultado de una llamada a la API: separa el "salió bien" del mensaje de error
// que devuelve el back, para que los controllers no tengan que leer status codes.
public record ApiResultado(bool Ok, string? Error = null)
{
    public static ApiResultado Exito() => new(true);
    public static ApiResultado Fallo(string error) => new(false, error);
}

public record ApiResultado<T>(bool Ok, T? Datos, string? Error = null)
{
    public static ApiResultado<T> Exito(T? datos) => new(true, datos);
    public static ApiResultado<T> Fallo(string error) => new(false, default, error);
}

using System.Collections.Concurrent;

namespace Clases_KioPlus.Logica;

// Cuenta los intentos fallidos de inicio de sesión por nombre de usuario.
// Al llegar a MaxIntentosFallidos el usuario tiene que esperar TiempoDeEspera.
// Vive en memoria a propósito: si la API se reinicia, los contadores arrancan de cero.
public class ControlIntentosLogin
{
    public const int MaxIntentosFallidos = 5;
    public static readonly TimeSpan TiempoDeEspera = TimeSpan.FromMinutes(1);

    private sealed class Estado
    {
        public int Fallos;
        public DateTime? EsperaHasta;
    }

    private readonly ConcurrentDictionary<string, Estado> _estados =
        new(StringComparer.OrdinalIgnoreCase);

    private static string Clave(string nombreUsuario) => (nombreUsuario ?? string.Empty).Trim();

    // True mientras dure la espera. Cuando se cumple el plazo, el usuario se limpia.
    public bool EstaEsperando(string nombreUsuario)
    {
        var clave = Clave(nombreUsuario);
        if (!_estados.TryGetValue(clave, out var estado)) return false;

        lock (estado)
        {
            if (estado.EsperaHasta is null) return false;
            if (DateTime.Now < estado.EsperaHasta) return true;
        }

        _estados.TryRemove(new KeyValuePair<string, Estado>(clave, estado));
        return false;
    }

    // En el último intento permitido arranca la espera y el contador vuelve a cero
    public void RegistrarFallo(string nombreUsuario)
    {
        var estado = _estados.GetOrAdd(Clave(nombreUsuario), _ => new Estado());

        lock (estado)
        {
            estado.Fallos++;
            if (estado.Fallos < MaxIntentosFallidos) return;

            estado.EsperaHasta = DateTime.Now.Add(TiempoDeEspera);
            estado.Fallos = 0;
        }
    }

    // Inicio de sesión correcto: se borran los fallos acumulados
    public void Reiniciar(string nombreUsuario) =>
        _estados.TryRemove(Clave(nombreUsuario), out _);
}

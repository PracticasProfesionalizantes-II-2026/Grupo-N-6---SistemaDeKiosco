namespace Clases_KioPlus.Logica.DTOs;

// Resultado de una operación de negocio que puede fallar por reglas de dominio
// (por ejemplo, stock insuficiente) y no solo por "no encontrado".
public record ResultadoOperacion(bool Ok, string? Error = null, int? Id = null)
{
    public static ResultadoOperacion Exito(int? id = null) => new(true, null, id);
    public static ResultadoOperacion NoEncontrado(string error) => new(false, error);
    public static ResultadoOperacion Invalido(string error) => new(false, error);
}

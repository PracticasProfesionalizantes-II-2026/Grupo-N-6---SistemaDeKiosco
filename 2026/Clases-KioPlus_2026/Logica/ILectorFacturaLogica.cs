using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Logica;

public interface ILectorFacturaLogica
{
    Task<(FacturaInterpretadaDto? Factura, string? Error)> Interpretar(byte[] pdf);
}

using Clases_KioPlus.Logica;

namespace Clases_KioPlus.Endpoints;

public static class CajaEndpoints
{
    public static void MapCajaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/caja").WithTags("Caja");

        // Saldo actual de la caja. Es lo único que el sistema expone de ella:
        // no hay gestión de caja, solo el saldo que mueven ventas y compras.
        grupo.MapGet("/", async (ICajaLogica logica) =>
            Results.Ok(new { saldoActual = await logica.SaldoActual() }));
    }
}

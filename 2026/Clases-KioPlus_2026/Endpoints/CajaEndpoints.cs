using Clases_KioPlus.Logica;

namespace Clases_KioPlus.Endpoints;

public static class CajaEndpoints
{
    public static void MapCajaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/caja").WithTags("Caja");

        // Saldo actual, o saldo a una fecha con ?fecha=2026-05-01
        grupo.MapGet("/", async (DateTime? fecha, ICajaLogica logica) =>
        {
            if (fecha.HasValue)
            {
                var saldo = await logica.SaldoAFecha(fecha.Value);
                return Results.Ok(new { fecha = fecha.Value.Date, saldo });
            }

            var saldoActual = await logica.SaldoActual();
            return Results.Ok(new { saldoActual });
        });
    }
}

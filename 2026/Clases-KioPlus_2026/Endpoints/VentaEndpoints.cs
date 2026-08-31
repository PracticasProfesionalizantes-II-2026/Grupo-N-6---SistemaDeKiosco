using Clases_KioPlus.Filters;
using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class VentaEndpoints
{
    public static void MapVentaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/ventas").WithTags("Ventas");

        // Listado con nombres de vendedor y cliente; todos los filtros son opcionales
        grupo.MapGet("/", async (
            DateTime? fechaDesde, DateTime? fechaHasta, int? idUsuario, int? idCliente,
            double? importeMayorA, double? importeMenorA,
            IVentaLogica logica) =>
            Results.Ok(await logica.ObtenerTodas(
                fechaDesde, fechaHasta, idUsuario, idCliente, importeMayorA, importeMenorA)));

        grupo.MapGet("/{id:int}", async (int id, IVentaLogica logica) =>
        {
            var venta = await logica.ObtenerPorId(id);
            return venta is null ? Results.NotFound() : Results.Ok(venta);
        });

        grupo.MapPost("/", async (VentaCreateDto dto, IVentaLogica logica) =>
        {
            var resultado = await logica.Crear(dto);
            if (resultado.Ok)
                return Results.Created($"/ventas/{resultado.Id}",
                    new { idVenta = resultado.Id, idUsuario = dto.IdUsuario });

            return resultado.Error!.Contains("no encontrad")
                ? Results.NotFound(new { mensaje = resultado.Error })
                : Results.BadRequest(new { mensaje = resultado.Error });
        }).AddEndpointFilter<ValidationFilter<VentaCreateDto>>();

        // Cierre de la venta: impacta la cuenta corriente del cliente si corresponde
        grupo.MapPost("/{id:int}/finalizar", async (int id, IVentaLogica logica) =>
        {
            var resultado = await logica.Finalizar(id);
            if (resultado.Ok) return Results.Ok(new { mensaje = "venta finalizada" });

            return resultado.Error!.Contains("no encontrad")
                ? Results.NotFound(new { mensaje = resultado.Error })
                : Results.BadRequest(new { mensaje = resultado.Error });
        });

        grupo.MapPut("/{id:int}", async (int id, VentaCreateDto dto, IVentaLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "venta actualizada" }) : Results.NotFound();
        }).AddEndpointFilter<ValidationFilter<VentaCreateDto>>();

        grupo.MapDelete("/{id:int}", async (int id, IVentaLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.Ok(new { mensaje = "venta eliminada" }) : Results.NotFound();
        });
    }
}

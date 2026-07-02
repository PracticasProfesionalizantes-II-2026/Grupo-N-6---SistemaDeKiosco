using Clases_KioPlus.Filters;
using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class VentaEndpoints
{
    public static void MapVentaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/ventas").WithTags("Ventas");

        // Listar todas, o filtrar si llega algún parámetro de filtro
        grupo.MapGet("/", async (
            DateTime? fechaHora, int? idUsuario, int? idCliente,
            double? importeMayorA, double? importeMenorA,
            IVentaLogica logica) =>
        {
            bool hayFiltro = fechaHora.HasValue || idUsuario.HasValue || idCliente.HasValue
                             || importeMayorA.HasValue || importeMenorA.HasValue;

            if (hayFiltro)
                return Results.Ok(await logica.Filtrar(fechaHora, idUsuario, idCliente, importeMayorA, importeMenorA));

            return Results.Ok(await logica.ObtenerTodas());
        });

        grupo.MapGet("/{id:int}", async (int id, IVentaLogica logica) =>
        {
            var venta = await logica.ObtenerPorId(id);
            return venta is null ? Results.NotFound() : Results.Ok(venta);
        });

        grupo.MapPost("/", async (VentaCreateDto dto, IVentaLogica logica) =>
        {
            var id = await logica.Crear(dto);
            return id is null
                ? Results.NotFound(new { mensaje = "usuario o cuenta corriente no encontrados" })
                : Results.Created($"/ventas/{id}", new { idVenta = id, idUsuario = dto.IdUsuario });
        }).AddEndpointFilter<ValidationFilter<VentaCreateDto>>();

        grupo.MapPut("/{id:int}", async (int id, VentaCreateDto dto, IVentaLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "venta actualizada" }) : Results.NotFound();
        }).AddEndpointFilter<ValidationFilter<VentaCreateDto>>();

        grupo.MapDelete("/{id:int}", async (int id, IVentaLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.NoContent() : Results.NotFound();
        });
    }
}

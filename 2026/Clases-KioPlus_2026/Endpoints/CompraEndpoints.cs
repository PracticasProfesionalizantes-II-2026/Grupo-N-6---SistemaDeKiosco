using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class CompraEndpoints
{
    public static void MapCompraEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/compras").WithTags("Compras");

        grupo.MapGet("/", async (ICompraLogica logica) =>
            Results.Ok(await logica.ObtenerTodas()));

        grupo.MapGet("/{id:int}", async (int id, ICompraLogica logica) =>
        {
            var compra = await logica.ObtenerPorId(id);
            return compra is null ? Results.NotFound() : Results.Ok(compra);
        });

        grupo.MapPost("/", async (CompraCreateDto dto, ICompraLogica logica) =>
        {
            var id = await logica.Crear(dto);
            return id is null
                ? Results.NotFound(new { mensaje = "proveedor no encontrado" })
                : Results.Created($"/compras/{id}", new { idCompraProveedor = id });
        });

        grupo.MapPut("/{id:int}", async (int id, CompraCreateDto dto, ICompraLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "compra actualizada" }) : Results.NotFound();
        });

        grupo.MapDelete("/{id:int}", async (int id, ICompraLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.Ok(new { mensaje = "compra eliminada" }) : Results.NotFound();
        });
    }
}

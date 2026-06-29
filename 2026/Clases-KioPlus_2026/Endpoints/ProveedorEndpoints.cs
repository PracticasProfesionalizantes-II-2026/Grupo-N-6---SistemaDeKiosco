using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class ProveedorEndpoints
{
    public static void MapProveedorEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/proveedores").WithTags("Proveedores");

        // Listar todos o filtrar por nombre con ?busqueda=
        grupo.MapGet("/", async (string? busqueda, IProveedorLogica logica) =>
            Results.Ok(await logica.ObtenerTodos(busqueda)));

        grupo.MapGet("/{id:int}", async (int id, IProveedorLogica logica) =>
        {
            var proveedor = await logica.ObtenerPorId(id);
            return proveedor is null ? Results.NotFound() : Results.Ok(proveedor);
        });

        grupo.MapPost("/", async (ProveedorCreateDto dto, IProveedorLogica logica) =>
        {
            var id = await logica.Crear(dto);
            return Results.Created($"/proveedores/{id}", new { idProveedor = id });
        });

        grupo.MapPut("/{id:int}", async (int id, ProveedorCreateDto dto, IProveedorLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "proveedor actualizado" }) : Results.NotFound();
        });

        grupo.MapDelete("/{id:int}", async (int id, IProveedorLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.Ok(new { mensaje = "proveedor eliminado" }) : Results.NotFound();
        });
    }
}

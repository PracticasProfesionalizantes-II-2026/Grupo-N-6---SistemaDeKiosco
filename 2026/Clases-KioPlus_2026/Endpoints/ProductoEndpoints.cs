using Clases_KioPlus.Filters;
using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class ProductoEndpoints
{
    public static void MapProductoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/productos").WithTags("Productos");

        // Listar / filtrar productos. Según el query string aplica un filtro distinto.
        grupo.MapGet("/", async (
            string? nombre, int? idCategoria, string? marca,
            bool? stockCritico, bool? proximoVencimiento,
            IProductoLogica logica) =>
        {
            if (stockCritico == true)
                return Results.Ok(await logica.ObtenerStockCritico());

            if (proximoVencimiento == true)
                return Results.Ok(await logica.ObtenerProximosAVencer());

            return Results.Ok(await logica.ObtenerTodos(nombre, idCategoria, marca));
        });

        // Productos más vendidos en un rango de fechas (criterio = cantidad | monto)
        grupo.MapGet("/mas-vendidos", async (
            DateTime fechaDesde, DateTime fechaHasta, string? criterio, int? limite,
            IProductoLogica logica) =>
        {
            var resultado = await logica.MasVendidos(
                fechaDesde, fechaHasta, criterio ?? "cantidad", limite ?? 5);
            return Results.Ok(resultado);
        });

        grupo.MapGet("/{id:int}", async (int id, IProductoLogica logica) =>
        {
            var producto = await logica.ObtenerPorId(id);
            return producto is null ? Results.NotFound() : Results.Ok(producto);
        });

        grupo.MapPost("/", async (ProductoCreateDto dto, IProductoLogica logica) =>
        {
            var id = await logica.Crear(dto);
            return Results.Created($"/productos/{id}", new { idProducto = id });
        }).AddEndpointFilter<ValidationFilter<ProductoCreateDto>>();

        grupo.MapPut("/{id:int}", async (int id, ProductoCreateDto dto, IProductoLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "producto actualizado" }) : Results.NotFound();
        }).AddEndpointFilter<ValidationFilter<ProductoCreateDto>>();

        grupo.MapDelete("/{id:int}", async (int id, IProductoLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.NoContent() : Results.NotFound();
        });
    }
}

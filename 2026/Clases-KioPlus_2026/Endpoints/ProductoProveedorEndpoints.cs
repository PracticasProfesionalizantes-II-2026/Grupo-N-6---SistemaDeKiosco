using Clases_KioPlus.Filters;
using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;

namespace Clases_KioPlus.Endpoints;

public static class ProductoProveedorEndpoints
{
    public static void MapProductoProveedorEndpoints(this IEndpointRouteBuilder app)
    {
        // La misma asociación leída desde el proveedor: sus productos con el precio
        app.MapGet("/proveedores/{idProveedor:int}/productos", async (int idProveedor, IProductoProveedorLogica logica) =>
        {
            var productos = await logica.ObtenerPorProveedor(idProveedor);
            return productos is null
                ? Results.NotFound(new { mensaje = "proveedor no encontrado" })
                : Results.Ok(productos);
        }).WithTags("ProductoProveedor");

        var grupo = app.MapGroup("/productos/{idProducto:int}/proveedores").WithTags("ProductoProveedor");

        grupo.MapGet("/", async (int idProducto, IProductoProveedorLogica logica) =>
            Results.Ok(await logica.ObtenerPorProducto(idProducto)));

        grupo.MapGet("/{id:int}", async (int idProducto, int id, IProductoProveedorLogica logica) =>
        {
            var pp = await logica.ObtenerPorId(id);
            return pp is null ? Results.NotFound() : Results.Ok(pp);
        });

        grupo.MapPost("/", async (int idProducto, ProductoProveedorCreateDto dto, IProductoProveedorLogica logica) =>
        {
            var resultado = await logica.Crear(idProducto, dto);
            if (resultado.Ok)
                return Results.Created($"/productos/{idProducto}/proveedores/{resultado.Id}",
                    new { idProductoProveedor = resultado.Id });

            return resultado.Error!.Contains("no encontrad")
                ? Results.NotFound(new { mensaje = resultado.Error })
                : Results.BadRequest(new { mensaje = resultado.Error });
        }).AddEndpointFilter<ValidationFilter<ProductoProveedorCreateDto>>();

        grupo.MapPut("/{id:int}", async (int idProducto, int id, ProductoProveedorUpdateDto dto, IProductoProveedorLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "producto proveedor actualizado" }) : Results.NotFound();
        }).AddEndpointFilter<ValidationFilter<ProductoProveedorUpdateDto>>();

        grupo.MapDelete("/{id:int}", async (int idProducto, int id, IProductoProveedorLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.NoContent() : Results.NotFound();
        });
    }
}

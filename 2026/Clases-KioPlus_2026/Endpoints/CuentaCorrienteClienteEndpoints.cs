using Clases_KioPlus.Logica;
using Clases_KioPlus.Logica.DTOs;
using Clases_KioPlus.Models;

namespace Clases_KioPlus.Endpoints;

public static class CuentaCorrienteClienteEndpoints
{
    public static void MapCuentaCorrienteClienteEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/cuentas-corrientes-clientes").WithTags("CuentasCorrientesClientes");

        // Listar todas o filtrar por nombre/apellido, dni/estado, o rango de monto adeudado
        grupo.MapGet("/", async (
            string? nombre, string? apellido, int? dni, string? estado,
            double? montoAdeudadoMin, double? montoAdeudadoMax,
            ICuentaCorrienteClienteLogica logica) =>
        {
            CuentaCorrienteCliente.EstadoDeuda? estadoEnum = null;
            if (!string.IsNullOrWhiteSpace(estado) &&
                Enum.TryParse<CuentaCorrienteCliente.EstadoDeuda>(estado, ignoreCase: true, out var parseado))
                estadoEnum = parseado;

            var resultado = await logica.ObtenerTodas(
                nombre, apellido, dni, estadoEnum, montoAdeudadoMin, montoAdeudadoMax);
            return Results.Ok(resultado);
        });

        grupo.MapGet("/{id:int}", async (int id, ICuentaCorrienteClienteLogica logica) =>
        {
            var cuenta = await logica.ObtenerPorId(id);
            return cuenta is null ? Results.NotFound() : Results.Ok(cuenta);
        });

        grupo.MapPost("/", async (CuentaCorrienteClienteCreateDto dto, ICuentaCorrienteClienteLogica logica) =>
        {
            var id = await logica.Crear(dto);
            return Results.Created($"/cuentas-corrientes-clientes/{id}", new { idCuentaCorrienteCliente = id });
        });

        grupo.MapPut("/{id:int}", async (int id, CuentaCorrienteClienteCreateDto dto, ICuentaCorrienteClienteLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "cuenta corriente actualizada" }) : Results.NotFound();
        });

        grupo.MapDelete("/{id:int}", async (int id, ICuentaCorrienteClienteLogica logica) =>
        {
            var ok = await logica.Eliminar(id);
            return ok ? Results.Ok(new { mensaje = "cuenta corriente eliminada" }) : Results.NotFound();
        });
    }
}

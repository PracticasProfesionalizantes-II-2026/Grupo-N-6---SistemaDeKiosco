using Clases_KioPlus.Filters;
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
        }).AddEndpointFilter<ValidationFilter<CuentaCorrienteClienteCreateDto>>();

        grupo.MapPut("/{id:int}", async (int id, CuentaCorrienteClienteCreateDto dto, ICuentaCorrienteClienteLogica logica) =>
        {
            var ok = await logica.Actualizar(id, dto);
            return ok ? Results.Ok(new { mensaje = "cuenta corriente actualizada" }) : Results.NotFound();
        }).AddEndpointFilter<ValidationFilter<CuentaCorrienteClienteCreateDto>>();

        // Registro de pago (icono $ del listado): cancela total o parcialmente la deuda
        grupo.MapPost("/{id:int}/pagos", async (int id, PagoCuentaCorrienteDto dto, ICuentaCorrienteClienteLogica logica) =>
        {
            var resultado = await logica.RegistrarPago(id, dto);
            if (resultado.Ok) return Results.Ok(new { mensaje = "pago registrado" });

            return resultado.Error!.Contains("no encontrad")
                ? Results.NotFound(new { mensaje = resultado.Error })
                : Results.BadRequest(new { mensaje = resultado.Error });
        }).AddEndpointFilter<ValidationFilter<PagoCuentaCorrienteDto>>();

        grupo.MapDelete("/{id:int}", async (int id, ICuentaCorrienteClienteLogica logica) =>
        {
            var resultado = await logica.Eliminar(id);
            if (resultado.Ok) return Results.Ok(new { mensaje = "cuenta corriente eliminada" });

            return resultado.Error!.Contains("no encontrad")
                ? Results.NotFound(new { mensaje = resultado.Error })
                : Results.BadRequest(new { mensaje = resultado.Error });
        });
    }
}

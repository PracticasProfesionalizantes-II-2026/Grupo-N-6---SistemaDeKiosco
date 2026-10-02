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

        // Ventas en cuenta corriente que el cliente todavía no pagó
        grupo.MapGet("/{id:int}/ventas-adeudadas", async (int id, IVentaLogica logicaVentas) =>
            Results.Ok(await logicaVentas.ObtenerAdeudadasPorCliente(id)));

        // Cancela la deuda completa y marca como pagadas todas las ventas adeudadas.
        // El cuerpo es opcional: sin fechaPago se toma la fecha de hoy.
        grupo.MapPost("/{id:int}/pagar-todo", async (int id, PagoDeudaTotalDto? dto, ICuentaCorrienteClienteLogica logica) =>
        {
            var resultado = await logica.PagarDeudaTotal(id, dto ?? new PagoDeudaTotalDto(null));
            if (resultado.Ok) return Results.Ok(new { mensaje = "deuda cancelada" });

            return resultado.Error!.Contains("no encontrad")
                ? Results.NotFound(new { mensaje = resultado.Error })
                : Results.BadRequest(new { mensaje = resultado.Error });
        });

        // Registro de pago a cuenta (icono $ del listado): baja el saldo por un monto libre
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

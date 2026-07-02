using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Middleware;

// Captura cualquier excepción no controlada: fallas de conexión a la base de datos
// devuelven 503 (servicio no disponible), el resto devuelve 500 (error interno).
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (EsFallaDeConexion(ex))
        {
            _logger.LogError(ex, "Falla de conexión a la base de datos");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { mensaje = "servicio no disponible" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { mensaje = "error interno del servidor" });
        }
    }

    private static bool EsFallaDeConexion(Exception ex) =>
        ex is SqlException
        || (ex is DbUpdateException dbEx && dbEx.InnerException is SqlException);
}

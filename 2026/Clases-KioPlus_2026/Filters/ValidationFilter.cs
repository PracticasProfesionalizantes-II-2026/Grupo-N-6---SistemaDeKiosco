using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Filters;

// Valida los DataAnnotations del DTO recibido en el body; si falla, devuelve 400
// en lugar de dejar que la lógica de negocio reciba datos incompletos o inválidos.
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var dto = context.Arguments.OfType<T>().FirstOrDefault();
        if (dto is not null)
        {
            var resultados = new List<ValidationResult>();
            var contexto = new ValidationContext(dto);
            if (!Validator.TryValidateObject(dto, contexto, resultados, validateAllProperties: true))
            {
                var errores = resultados.Select(r => r.ErrorMessage).ToArray();
                return Results.BadRequest(new { mensaje = "datos inválidos", errores });
            }
        }

        return await next(context);
    }
}

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrdersManagement.Application.Exceptions;
using OrdersManagement.Domain.Exceptions;

namespace OrdersManagement.WebApi.Middleware;

/// <summary>
/// Manejo centralizado de errores: validaciones como 400 y cualquier otro error como 500 sin detalles internos.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            DomainValidationException ex => CreateValidationProblem(ex.Errors),
            ValidationException ex => CreateValidationProblem(ex.Errors),
            _ => null
        };

        if (problem is null)
        {
            logger.LogError(exception, "Error no controlado al procesar {Method} {Path}.",
                httpContext.Request.Method, httpContext.Request.Path);

            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor",
                Detail = "Ocurrió un error inesperado. Intente nuevamente más tarde."
            };
        }
        else
        {
            logger.LogWarning("Solicitud rechazada por validación: {Errors}", problem.Extensions["errors"]);
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateValidationProblem(IReadOnlyList<string> errors)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "La solicitud no es válida",
            Detail = string.Join(" ", errors)
        };
        problem.Extensions["errors"] = errors;

        return problem;
    }
}

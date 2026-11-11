namespace OrdersManagement.Application.Exceptions;

/// <summary>
/// Error de validación de la solicitud (por ejemplo, parámetros de paginación inválidos).
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(params string[] errors)
        : base("La solicitud no es válida.")
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}

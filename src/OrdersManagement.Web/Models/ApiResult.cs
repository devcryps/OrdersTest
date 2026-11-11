namespace OrdersManagement.Web.Models;

/// <summary>
/// Resultado de una llamada a la API: el valor si fue exitosa o los mensajes de error en español.
/// </summary>
public class ApiResult<T>
{
    public bool Success { get; private init; }

    public T? Value { get; private init; }

    public IReadOnlyList<string> Errors { get; private init; } = Array.Empty<string>();

    public static ApiResult<T> Ok(T value) => new() { Success = true, Value = value };

    public static ApiResult<T> Fail(params string[] errors) => new() { Success = false, Errors = errors };
}

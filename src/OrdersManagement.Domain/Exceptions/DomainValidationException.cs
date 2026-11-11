namespace OrdersManagement.Domain.Exceptions;

/// <summary>
/// Se lanza cuando una entidad incumple una o varias reglas de negocio.
/// </summary>
public class DomainValidationException : Exception
{
    public DomainValidationException(IEnumerable<string> errors)
        : base("Se incumplieron reglas de negocio.")
    {
        Errors = errors.ToList();
    }

    public IReadOnlyList<string> Errors { get; }
}

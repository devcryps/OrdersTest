namespace OrdersManagement.Infrastructure.Persistence.Oracle;

public class OracleOptions
{
    public const string SectionName = "Oracle";

    public string ConnectionString { get; set; } = string.Empty;
}

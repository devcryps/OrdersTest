namespace OrdersManagement.Infrastructure.Caching;

public class CacheOptions
{
    public const string SectionName = "Cache";

    public int ListExpirationSeconds { get; set; } = 60;
}

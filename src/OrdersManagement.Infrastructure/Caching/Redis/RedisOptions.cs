namespace OrdersManagement.Infrastructure.Caching.Redis;

public class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";
}

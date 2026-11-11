using System.Text.Json;
using Microsoft.Extensions.Options;
using OrdersManagement.Application.DTOs;
using OrdersManagement.Application.Interfaces;
using StackExchange.Redis;

namespace OrdersManagement.Infrastructure.Caching.Redis;

/// <summary>
/// Caché Redis del listado. Las claves incluyen una versión; invalidar es incrementarla.
/// </summary>
public class RedisOrderCache(IConnectionMultiplexer redis, IOptions<CacheOptions> options) : IOrderCache
{
    private const string VersionKey = "orders:list:version";

    private readonly TimeSpan _expiration = TimeSpan.FromSeconds(options.Value.ListExpirationSeconds);

    public async Task<PagedResult<OrderResponse>?> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var database = redis.GetDatabase();
        var key = await BuildKeyAsync(database, pageNumber, pageSize);

        var json = await database.StringGetAsync(key);

        return json.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<PagedResult<OrderResponse>>(json.ToString());
    }

    public async Task SetPageAsync(int pageNumber, int pageSize, PagedResult<OrderResponse> page, CancellationToken cancellationToken)
    {
        var database = redis.GetDatabase();
        var key = await BuildKeyAsync(database, pageNumber, pageSize);

        await database.StringSetAsync(key, JsonSerializer.Serialize(page), _expiration);
    }

    public async Task InvalidateListAsync(CancellationToken cancellationToken)
    {
        await redis.GetDatabase().StringIncrementAsync(VersionKey);
    }

    private static async Task<string> BuildKeyAsync(IDatabase database, int pageNumber, int pageSize)
    {
        var version = await database.StringGetAsync(VersionKey);
        var versionNumber = version.IsNullOrEmpty ? "0" : version.ToString();

        return $"orders:list:v{versionNumber}:page{pageNumber}:size{pageSize}";
    }
}

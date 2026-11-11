using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrdersManagement.Application.DTOs;
using OrdersManagement.Application.Interfaces;

namespace OrdersManagement.Infrastructure.Caching.InMemory;

/// <summary>
/// Caché local con IMemoryCache. Usa la misma estrategia de versionado de claves que la implementación Redis.
/// </summary>
public class MemoryOrderCache(IMemoryCache cache, IOptions<CacheOptions> options) : IOrderCache
{
    private readonly TimeSpan _expiration = TimeSpan.FromSeconds(options.Value.ListExpirationSeconds);
    private int _version;

    public Task<PagedResult<OrderResponse>?> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        cache.TryGetValue(BuildKey(pageNumber, pageSize), out PagedResult<OrderResponse>? page);
        return Task.FromResult(page);
    }

    public Task SetPageAsync(int pageNumber, int pageSize, PagedResult<OrderResponse> page, CancellationToken cancellationToken)
    {
        cache.Set(BuildKey(pageNumber, pageSize), page, _expiration);
        return Task.CompletedTask;
    }

    public Task InvalidateListAsync(CancellationToken cancellationToken)
    {
        // Cambiar la versión deja inalcanzables las claves anteriores; expiran solas.
        Interlocked.Increment(ref _version);
        return Task.CompletedTask;
    }

    private string BuildKey(int pageNumber, int pageSize) =>
        $"orders:list:v{Volatile.Read(ref _version)}:page{pageNumber}:size{pageSize}";
}

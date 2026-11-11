using OrdersManagement.Application.DTOs;

namespace OrdersManagement.Application.Interfaces;

public interface IOrderCache
{
    Task<PagedResult<OrderResponse>?> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task SetPageAsync(int pageNumber, int pageSize, PagedResult<OrderResponse> page, CancellationToken cancellationToken);

    /// <summary>
    /// Invalida todas las páginas cacheadas del listado.
    /// </summary>
    Task InvalidateListAsync(CancellationToken cancellationToken);
}

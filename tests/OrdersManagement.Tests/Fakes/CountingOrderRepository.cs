using OrdersManagement.Application.Interfaces;
using OrdersManagement.Domain.Entities;
using OrdersManagement.Domain.Enums;
using OrdersManagement.Infrastructure.Persistence.InMemory;

namespace OrdersManagement.Tests.Fakes;

/// <summary>
/// Envuelve el repositorio en memoria para contar las llamadas que importan en las pruebas.
/// </summary>
public class CountingOrderRepository : IOrderRepository
{
    private readonly InMemoryOrderRepository _inner = new();

    public int UpdateStatusCalls { get; private set; }

    public int CountCalls { get; private set; }

    public Task AddAsync(Order order, CancellationToken cancellationToken) => _inner.AddAsync(order, cancellationToken);

    public Task<IReadOnlyList<Order>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken) =>
        _inner.GetPagedAsync(pageNumber, pageSize, cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        CountCalls++;
        return _inner.CountAsync(cancellationToken);
    }

    public Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken) => _inner.GetByIdAsync(id, cancellationToken);

    public Task UpdateStatusAsync(long id, OrderStatus status, CancellationToken cancellationToken)
    {
        UpdateStatusCalls++;
        return _inner.UpdateStatusAsync(id, status, cancellationToken);
    }
}

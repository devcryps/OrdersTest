using OrdersManagement.Application.Interfaces;
using OrdersManagement.Domain.Entities;
using OrdersManagement.Domain.Enums;

namespace OrdersManagement.Infrastructure.Persistence.InMemory;

/// <summary>
/// Repositorio en memoria para ejecutar la aplicación sin Oracle. Es seguro para uso concurrente.
/// </summary>
public class InMemoryOrderRepository : IOrderRepository
{
    private readonly object _lock = new();
    private readonly Dictionary<long, Order> _orders = new();
    private long _lastOrderId;
    private long _lastItemId;

    public Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            order.Id = ++_lastOrderId;
            foreach (var item in order.Items)
            {
                item.Id = ++_lastItemId;
                item.OrderId = order.Id;
            }

            _orders[order.Id] = Copy(order, order.Status);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Order>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            IReadOnlyList<Order> page = _orders.Values
                .OrderByDescending(o => o.CreatedAt)
                .ThenByDescending(o => o.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => Copy(o, o.Status))
                .ToList();

            return Task.FromResult(page);
        }
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return Task.FromResult(_orders.Count);
        }
    }

    public Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            var order = _orders.TryGetValue(id, out var stored) ? Copy(stored, stored.Status) : null;
            return Task.FromResult(order);
        }
    }

    public Task UpdateStatusAsync(long id, OrderStatus status, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (_orders.TryGetValue(id, out var stored))
            {
                _orders[id] = Copy(stored, status);
            }
        }

        return Task.CompletedTask;
    }

    // Se guardan y devuelven copias para imitar el comportamiento de una base de datos real.
    private static Order Copy(Order order, OrderStatus status)
    {
        var items = order.Items.Select(i => new OrderItem
        {
            Id = i.Id,
            OrderId = i.OrderId,
            Description = i.Description,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        });

        return Order.Restore(order.Id, order.Customer, items, order.Total, status, order.CreatedAt);
    }
}

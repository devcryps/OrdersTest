using OrdersManagement.Domain.Entities;
using OrdersManagement.Domain.Enums;

namespace OrdersManagement.Application.Interfaces;

public interface IOrderRepository
{
    /// <summary>
    /// Guarda el pedido y sus ítems y asigna los identificadores generados.
    /// </summary>
    Task AddAsync(Order order, CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene una página ordenada por fecha de creación descendente y luego por Id descendente.
    /// </summary>
    Task<IReadOnlyList<Order>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<int> CountAsync(CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task UpdateStatusAsync(long id, OrderStatus status, CancellationToken cancellationToken);
}

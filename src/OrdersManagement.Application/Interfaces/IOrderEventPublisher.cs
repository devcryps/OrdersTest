using OrdersManagement.Application.Events;

namespace OrdersManagement.Application.Interfaces;

public interface IOrderEventPublisher
{
    Task PublishOrderCreatedAsync(OrderCreated orderCreated, CancellationToken cancellationToken);
}

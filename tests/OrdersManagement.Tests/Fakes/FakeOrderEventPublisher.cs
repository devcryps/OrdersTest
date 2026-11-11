using OrdersManagement.Application.Events;
using OrdersManagement.Application.Interfaces;

namespace OrdersManagement.Tests.Fakes;

public class FakeOrderEventPublisher : IOrderEventPublisher
{
    public List<OrderCreated> Published { get; } = new();

    public Task PublishOrderCreatedAsync(OrderCreated orderCreated, CancellationToken cancellationToken)
    {
        Published.Add(orderCreated);
        return Task.CompletedTask;
    }
}

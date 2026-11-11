using System.Threading.Channels;
using OrdersManagement.Application.Events;
using OrdersManagement.Application.Interfaces;

namespace OrdersManagement.Infrastructure.Messaging.InMemory;

/// <summary>
/// Publicador local basado en System.Threading.Channels para desarrollo sin RabbitMQ.
/// </summary>
public class ChannelOrderEventPublisher(Channel<OrderCreated> channel) : IOrderEventPublisher
{
    public async Task PublishOrderCreatedAsync(OrderCreated orderCreated, CancellationToken cancellationToken)
    {
        await channel.Writer.WriteAsync(orderCreated, cancellationToken);
    }
}

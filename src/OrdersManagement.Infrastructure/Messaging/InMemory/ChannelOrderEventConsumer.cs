using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using OrdersManagement.Application.Events;
using OrdersManagement.Application.Interfaces;

namespace OrdersManagement.Infrastructure.Messaging.InMemory;

public class ChannelOrderEventConsumer(Channel<OrderCreated> channel, ILogger<ChannelOrderEventConsumer> logger) : IOrderEventConsumer
{
    public async Task ConsumeAsync(Func<OrderCreated, CancellationToken, Task> handler, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var orderCreated in channel.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    await handler(orderCreated, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Error al procesar el evento OrderCreated del pedido {OrderId}; el mensaje se descarta.",
                        orderCreated.OrderId);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Cierre normal de la aplicación.
        }
    }
}

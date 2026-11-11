using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrdersManagement.Application.Events;
using OrdersManagement.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrdersManagement.Infrastructure.Messaging.RabbitMq;

public class RabbitMqOrderEventConsumer(RabbitMqConnectionProvider connectionProvider, ILogger<RabbitMqOrderEventConsumer> logger) : IOrderEventConsumer
{
    public async Task ConsumeAsync(Func<OrderCreated, CancellationToken, Task> handler, CancellationToken cancellationToken)
    {
        using var channel = connectionProvider.CreateChannel();

        // Un mensaje a la vez por consumidor, con confirmación manual.
        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, args) => await HandleMessageAsync(channel, args, handler, cancellationToken);

        channel.BasicConsume(queue: connectionProvider.QueueName, autoAck: false, consumer: consumer);
        logger.LogInformation("Consumidor RabbitMQ escuchando la cola {QueueName}.", connectionProvider.QueueName);

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Cierre normal: al cerrar el canal, los mensajes sin confirmar vuelven a la cola.
        }
    }

    private async Task HandleMessageAsync(
        IModel channel,
        BasicDeliverEventArgs args,
        Func<OrderCreated, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        OrderCreated? orderCreated;

        try
        {
            orderCreated = JsonSerializer.Deserialize<OrderCreated>(args.Body.Span);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Mensaje con formato inválido en la cola; se descarta sin reencolar.");
            channel.BasicReject(args.DeliveryTag, requeue: false);
            return;
        }

        if (orderCreated is null)
        {
            logger.LogError("Mensaje vacío en la cola; se descarta sin reencolar.");
            channel.BasicReject(args.DeliveryTag, requeue: false);
            return;
        }

        try
        {
            await handler(orderCreated, cancellationToken);
            channel.BasicAck(args.DeliveryTag, multiple: false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Sin confirmar: el mensaje volverá a entregarse cuando la aplicación se reinicie.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al procesar el evento OrderCreated del pedido {OrderId}; se rechaza sin reencolar.",
                orderCreated.OrderId);
            channel.BasicReject(args.DeliveryTag, requeue: false);
        }
    }
}

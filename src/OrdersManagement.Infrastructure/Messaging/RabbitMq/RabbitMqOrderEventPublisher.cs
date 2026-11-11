using System.Text.Json;
using OrdersManagement.Application.Events;
using OrdersManagement.Application.Interfaces;

namespace OrdersManagement.Infrastructure.Messaging.RabbitMq;

public class RabbitMqOrderEventPublisher(RabbitMqConnectionProvider connectionProvider) : IOrderEventPublisher
{
    private static readonly TimeSpan ConfirmTimeout = TimeSpan.FromSeconds(5);

    public Task PublishOrderCreatedAsync(OrderCreated orderCreated, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Los canales de RabbitMQ no son seguros entre hilos: se usa uno por publicación.
        using var channel = connectionProvider.CreateChannel();
        channel.ConfirmSelect();

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        var body = JsonSerializer.SerializeToUtf8Bytes(orderCreated);
        channel.BasicPublish(exchange: string.Empty, routingKey: connectionProvider.QueueName, mandatory: false, basicProperties: properties, body: body);
        channel.WaitForConfirmsOrDie(ConfirmTimeout);

        return Task.CompletedTask;
    }
}

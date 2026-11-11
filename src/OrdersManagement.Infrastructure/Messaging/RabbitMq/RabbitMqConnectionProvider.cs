using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace OrdersManagement.Infrastructure.Messaging.RabbitMq;

/// <summary>
/// Mantiene una única conexión a RabbitMQ (creada de forma perezosa) y entrega canales con la cola ya declarada.
/// </summary>
public sealed class RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options) : IDisposable
{
    private readonly RabbitMqOptions _options = options.Value;
    private readonly object _lock = new();
    private IConnection? _connection;

    public string QueueName => _options.QueueName;

    /// <summary>
    /// Crea un canal nuevo y declara la cola durable. Quien lo recibe es responsable de cerrarlo.
    /// </summary>
    public IModel CreateChannel()
    {
        var channel = GetConnection().CreateModel();
        channel.QueueDeclare(queue: _options.QueueName, durable: true, exclusive: false, autoDelete: false);
        return channel;
    }

    private IConnection GetConnection()
    {
        lock (_lock)
        {
            _connection ??= new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                ClientProvidedName = "OrdersManagement"
            }.CreateConnection();

            return _connection;
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}

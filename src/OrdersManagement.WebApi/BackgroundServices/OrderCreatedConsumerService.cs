using OrdersManagement.Application.Events;
using OrdersManagement.Application.Interfaces;

namespace OrdersManagement.WebApi.BackgroundServices;

/// <summary>
/// Escucha eventos OrderCreated y confirma el pedido mediante la capa Application.
/// </summary>
public class OrderCreatedConsumerService(
    IOrderEventConsumer consumer,
    IServiceScopeFactory scopeFactory,
    ILogger<OrderCreatedConsumerService> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Consumidor de eventos OrderCreated iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await consumer.ConsumeAsync(HandleAsync, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Por ejemplo, el broker no está disponible: se reintenta a intervalo fijo.
                logger.LogError(ex, "Falló el consumidor de eventos; se reintentará en {Seconds} segundos.",
                    RetryDelay.TotalSeconds);

                await DelayAsync(stoppingToken);
            }
        }

        logger.LogInformation("Consumidor de eventos OrderCreated detenido.");
    }

    private static async Task DelayAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(RetryDelay, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Cierre de la aplicación durante la espera.
        }
    }

    private async Task HandleAsync(OrderCreated orderCreated, CancellationToken cancellationToken)
    {
        logger.LogInformation("Evento OrderCreated recibido para el pedido {OrderId}.", orderCreated.OrderId);

        using var scope = scopeFactory.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        await orderService.ConfirmAsync(orderCreated.OrderId, cancellationToken);
    }
}


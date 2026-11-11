using OrdersManagement.Application.Events;

namespace OrdersManagement.Application.Interfaces;

public interface IOrderEventConsumer
{
    /// <summary>
    /// Escucha eventos OrderCreated hasta que se cancele el token e invoca el manejador por cada uno.
    /// Si el manejador falla, el mensaje se descarta sin reencolarse.
    /// </summary>
    Task ConsumeAsync(Func<OrderCreated, CancellationToken, Task> handler, CancellationToken cancellationToken);
}

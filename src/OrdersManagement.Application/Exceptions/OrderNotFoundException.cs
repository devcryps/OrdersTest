namespace OrdersManagement.Application.Exceptions;

public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(long orderId)
        : base($"No se encontró el pedido {orderId}.")
    {
        OrderId = orderId;
    }

    public long OrderId { get; }
}

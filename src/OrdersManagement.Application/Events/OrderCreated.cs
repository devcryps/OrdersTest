namespace OrdersManagement.Application.Events;

/// <summary>
/// Evento publicado cuando se crea un pedido.
/// </summary>
public record OrderCreated(long OrderId, DateTime CreatedAt);

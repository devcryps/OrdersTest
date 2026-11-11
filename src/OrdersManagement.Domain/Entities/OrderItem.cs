namespace OrdersManagement.Domain.Entities;

/// <summary>
/// Línea de detalle de un pedido.
/// </summary>
public class OrderItem
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Subtotal => Quantity * UnitPrice;
}

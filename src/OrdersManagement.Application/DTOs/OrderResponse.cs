namespace OrdersManagement.Application.DTOs;

public class OrderResponse
{
    public long Id { get; set; }

    public string Customer { get; set; } = string.Empty;

    public List<OrderItemResponse> Items { get; set; } = new();

    public decimal Total { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class OrderItemResponse
{
    public long Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}

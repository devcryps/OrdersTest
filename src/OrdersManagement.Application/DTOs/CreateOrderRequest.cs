namespace OrdersManagement.Application.DTOs;

public class CreateOrderRequest
{
    public string? Customer { get; set; }

    public List<CreateOrderItemRequest>? Items { get; set; }

    public decimal Total { get; set; }
}

public class CreateOrderItemRequest
{
    public string? Description { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}

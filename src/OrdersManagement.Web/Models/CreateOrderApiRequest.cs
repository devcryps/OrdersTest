namespace OrdersManagement.Web.Models;

/// <summary>
/// Cuerpo enviado a POST /api/pedidos.
/// </summary>
public class CreateOrderApiRequest
{
    public string Customer { get; set; } = string.Empty;

    public List<CreateOrderItemApiRequest> Items { get; set; } = new();

    public decimal Total { get; set; }
}

public class CreateOrderItemApiRequest
{
    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}

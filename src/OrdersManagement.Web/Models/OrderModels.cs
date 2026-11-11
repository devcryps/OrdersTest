namespace OrdersManagement.Web.Models;

/// <summary>
/// Pedido tal como lo devuelve la API.
/// </summary>
public class OrderModel
{
    public long Id { get; set; }

    public string Customer { get; set; } = string.Empty;

    public List<OrderItemModel> Items { get; set; } = new();

    public decimal Total { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class OrderItemModel
{
    public long Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}

public class PagedResultModel<T>
{
    public List<T> Items { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}

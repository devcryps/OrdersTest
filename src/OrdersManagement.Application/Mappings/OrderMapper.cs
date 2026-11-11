using OrdersManagement.Application.DTOs;
using OrdersManagement.Domain.Entities;

namespace OrdersManagement.Application.Mappings;

/// <summary>
/// Conversiones manuales entre DTOs y entidades de dominio.
/// </summary>
public static class OrderMapper
{
    public static Order ToEntity(CreateOrderRequest request, DateTime createdAt)
    {
        var items = (request.Items ?? new List<CreateOrderItemRequest>())
            .Select(item => new OrderItem
            {
                Description = item.Description?.Trim() ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });

        return Order.Create(request.Customer, items, request.Total, createdAt);
    }

    public static OrderResponse ToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            Customer = order.Customer,
            Total = order.Total,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(ToResponse).ToList()
        };
    }

    private static OrderItemResponse ToResponse(OrderItem item)
    {
        return new OrderItemResponse
        {
            Id = item.Id,
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        };
    }
}

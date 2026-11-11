using OrdersManagement.Domain.Enums;
using OrdersManagement.Domain.Exceptions;

namespace OrdersManagement.Domain.Entities;

/// <summary>
/// Pedido de un cliente. Concentra las reglas de negocio del pedido.
/// </summary>
public class Order
{
    private readonly List<OrderItem> _items = new();

    private Order()
    {
    }

    public long Id { get; set; }

    public string Customer { get; private set; } = string.Empty;

    public IReadOnlyList<OrderItem> Items => _items;

    public decimal Total { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Crea un pedido nuevo en estado Pending validando todas las reglas de negocio.
    /// </summary>
    /// <exception cref="DomainValidationException">Si algún dato incumple las reglas.</exception>
    public static Order Create(string? customer, IEnumerable<OrderItem>? items, decimal total, DateTime createdAt)
    {
        var itemList = items?.ToList() ?? new List<OrderItem>();
        var errors = Validate(customer, itemList, total);

        if (errors.Count > 0)
        {
            throw new DomainValidationException(errors);
        }

        var order = new Order
        {
            Customer = customer!.Trim(),
            Total = total,
            Status = OrderStatus.Pending,
            CreatedAt = createdAt
        };
        order._items.AddRange(itemList);

        return order;
    }

    /// <summary>
    /// Reconstruye un pedido existente (por ejemplo, leído de persistencia) sin volver a validarlo.
    /// </summary>
    public static Order Restore(long id, string customer, IEnumerable<OrderItem> items, decimal total, OrderStatus status, DateTime createdAt)
    {
        var order = new Order
        {
            Id = id,
            Customer = customer,
            Total = total,
            Status = status,
            CreatedAt = createdAt
        };
        order._items.AddRange(items);

        return order;
    }

    /// <summary>
    /// Confirma el pedido. Es idempotente: si ya estaba confirmado no modifica nada.
    /// </summary>
    /// <returns>true si el estado cambió; false si ya estaba confirmado.</returns>
    public bool Confirm()
    {
        if (Status == OrderStatus.Confirmed)
        {
            return false;
        }

        Status = OrderStatus.Confirmed;
        return true;
    }

    private static List<string> Validate(string? customer, List<OrderItem> items, decimal total)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(customer))
        {
            errors.Add("El cliente es obligatorio.");
        }

        if (items.Count == 0)
        {
            errors.Add("El pedido debe tener al menos un ítem.");
        }

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Description))
            {
                errors.Add("Cada ítem debe tener una descripción.");
            }

            if (item.Quantity <= 0)
            {
                errors.Add("La cantidad de cada ítem debe ser mayor que cero.");
            }

            if (item.UnitPrice < 0)
            {
                errors.Add("El precio unitario de un ítem no puede ser negativo.");
            }
        }

        if (total <= 0)
        {
            errors.Add("El total debe ser mayor que cero.");
        }
        else if (items.Count > 0 && total != items.Sum(i => i.Subtotal))
        {
            errors.Add("El total no coincide con la suma de cantidad por precio unitario de los ítems.");
        }

        return errors.Distinct().ToList();
    }
}

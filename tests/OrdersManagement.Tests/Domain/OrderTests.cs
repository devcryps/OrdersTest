using OrdersManagement.Domain.Entities;
using OrdersManagement.Domain.Enums;
using OrdersManagement.Domain.Exceptions;

namespace OrdersManagement.Tests.Domain;

public class OrderTests
{
    private static OrderItem Item(string description = "Producto", int quantity = 1, decimal unitPrice = 10m) =>
        new() { Description = description, Quantity = quantity, UnitPrice = unitPrice };

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyCustomer_Throws(string? customer)
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            Order.Create(customer, new[] { Item() }, 10m, DateTime.UtcNow));

        Assert.Contains("El cliente es obligatorio.", ex.Errors);
    }

    [Fact]
    public void Create_WithoutItems_Throws()
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            Order.Create("Ana", Array.Empty<OrderItem>(), 10m, DateTime.UtcNow));

        Assert.Contains("El pedido debe tener al menos un ítem.", ex.Errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Create_WithInvalidQuantity_Throws(int quantity)
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            Order.Create("Ana", new[] { Item(quantity: quantity) }, 10m, DateTime.UtcNow));

        Assert.Contains("La cantidad de cada ítem debe ser mayor que cero.", ex.Errors);
    }

    [Fact]
    public void Create_WithNegativeUnitPrice_Throws()
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            Order.Create("Ana", new[] { Item(unitPrice: -1m), Item(unitPrice: 5m) }, 4m, DateTime.UtcNow));

        Assert.Contains("El precio unitario de un ítem no puede ser negativo.", ex.Errors);
    }

    [Fact]
    public void Create_WithItemWithoutDescription_Throws()
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            Order.Create("Ana", new[] { Item(description: " ") }, 10m, DateTime.UtcNow));

        Assert.Contains("Cada ítem debe tener una descripción.", ex.Errors);
    }

    [Fact]
    public void Create_WithInconsistentTotal_Throws()
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            Order.Create("Ana", new[] { Item(quantity: 2, unitPrice: 10m) }, 25m, DateTime.UtcNow));

        Assert.Contains("El total no coincide con la suma de cantidad por precio unitario de los ítems.", ex.Errors);
    }

    [Fact]
    public void Create_WithZeroTotal_Throws()
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            Order.Create("Ana", new[] { Item(unitPrice: 0m) }, 0m, DateTime.UtcNow));

        Assert.Contains("El total debe ser mayor que cero.", ex.Errors);
    }

    [Fact]
    public void Create_WithValidData_StartsPending()
    {
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var order = Order.Create(" Ana ", new[] { Item(quantity: 2, unitPrice: 10.5m), Item(unitPrice: 4m) }, 25m, createdAt);

        Assert.Equal("Ana", order.Customer);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(25m, order.Total);
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(createdAt, order.CreatedAt);
    }

    [Fact]
    public void Confirm_ChangesStatusToConfirmed()
    {
        var order = Order.Create("Ana", new[] { Item() }, 10m, DateTime.UtcNow);

        var changed = order.Confirm();

        Assert.True(changed);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_DoesNothing()
    {
        var order = Order.Create("Ana", new[] { Item() }, 10m, DateTime.UtcNow);
        order.Confirm();

        var changed = order.Confirm();

        Assert.False(changed);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }
}

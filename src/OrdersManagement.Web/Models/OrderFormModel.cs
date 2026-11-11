using System.ComponentModel.DataAnnotations;

namespace OrdersManagement.Web.Models;

/// <summary>
/// Modelo del formulario de creación de pedidos.
/// </summary>
public class OrderFormModel
{
    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre del cliente no puede superar los 200 caracteres.")]
    public string Customer { get; set; } = string.Empty;

    public List<OrderItemFormModel> Items { get; } = new();

    public decimal Total => Items.Sum(item => item.Subtotal);
}

public class OrderItemFormModel
{
    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public decimal UnitPrice { get; set; }

    public decimal Subtotal => Quantity * UnitPrice;
}

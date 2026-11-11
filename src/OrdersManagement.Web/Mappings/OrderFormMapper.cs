using OrdersManagement.Web.Models;

namespace OrdersManagement.Web.Mappings;

/// <summary>
/// Conversión manual del formulario al modelo que espera la API.
/// </summary>
public static class OrderFormMapper
{
    public static CreateOrderApiRequest ToApiRequest(OrderFormModel form)
    {
        return new CreateOrderApiRequest
        {
            Customer = form.Customer.Trim(),
            Total = form.Total,
            Items = form.Items
                .Select(item => new CreateOrderItemApiRequest
                {
                    Description = item.Description.Trim(),
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                })
                .ToList()
        };
    }
}

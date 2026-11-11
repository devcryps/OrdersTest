using Microsoft.AspNetCore.Components;
using OrdersManagement.Web.Models;
using OrdersManagement.Web.Services;

namespace OrdersManagement.Web.Components.Pages;

public partial class NewOrder
{
    [Inject] private OrderApiService OrderApi { get; set; } = default!;

    private OrderFormModel order = new();
    private OrderItemFormModel newItem = new();
    private List<string> errors = new();
    private string? successMessage;
    private string? itemError;
    private bool isSaving;

    private void AddItem()
    {
        itemError = null;

        if (string.IsNullOrWhiteSpace(newItem.Description))
        {
            itemError = "La descripción del ítem es obligatoria.";
        }
        else if (newItem.Quantity <= 0)
        {
            itemError = "La cantidad debe ser mayor que cero.";
        }
        else if (newItem.UnitPrice < 0)
        {
            itemError = "El precio unitario no puede ser negativo.";
        }

        if (itemError is not null)
        {
            return;
        }

        order.Items.Add(newItem);
        newItem = new OrderItemFormModel();
    }

    private void RemoveItem(OrderItemFormModel item) => order.Items.Remove(item);

    private async Task SaveAsync()
    {
        errors.Clear();
        successMessage = null;

        if (order.Items.Count == 0)
        {
            errors.Add("Agregue al menos un ítem al pedido.");
            return;
        }

        if (order.Total <= 0)
        {
            errors.Add("El total del pedido debe ser mayor que cero.");
            return;
        }

        isSaving = true;
        var result = await OrderApi.CreateOrderAsync(order);
        isSaving = false;

        if (result.Success)
        {
            successMessage = $"Pedido {result.Value!.Id} creado correctamente.";
            order = new OrderFormModel();
            newItem = new OrderItemFormModel();
        }
        else
        {
            errors.AddRange(result.Errors);
        }
    }
}

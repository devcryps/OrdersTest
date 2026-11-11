using Microsoft.AspNetCore.Components;
using OrdersManagement.Web.Models;
using OrdersManagement.Web.Services;

namespace OrdersManagement.Web.Components.Pages;

public partial class Orders
{
    private const int PageSize = 10;

    [Inject] private OrderApiService OrderApi { get; set; } = default!;

    private PagedResultModel<OrderModel>? ordersPage;
    private int pageNumber = 1;
    private bool isLoading;
    private string? errorMessage;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        isLoading = true;
        errorMessage = null;

        var result = await OrderApi.GetOrdersAsync(pageNumber, PageSize);

        if (result.Success)
        {
            ordersPage = result.Value;
        }
        else
        {
            errorMessage = string.Join(" ", result.Errors);
        }

        isLoading = false;
    }

    private async Task PreviousPageAsync()
    {
        pageNumber--;
        await LoadAsync();
    }

    private async Task NextPageAsync()
    {
        pageNumber++;
        await LoadAsync();
    }

    private static string StatusText(string status) => status switch
    {
        "Pending" => "Pendiente",
        "Confirmed" => "Confirmado",
        _ => status
    };

    private static string StatusCss(string status) => status == "Confirmed" ? "badge-success" : "badge-warning";
}

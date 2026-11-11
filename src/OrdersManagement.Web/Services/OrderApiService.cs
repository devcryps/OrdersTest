using System.Text.Json;
using OrdersManagement.Web.Mappings;
using OrdersManagement.Web.Models;

namespace OrdersManagement.Web.Services;

/// <summary>
/// Cliente HTTP de la Web API de pedidos
/// </summary>
public class OrderApiService(HttpClient httpClient, ILogger<OrderApiService> logger)
{
    private const string ConnectionError =
        "No fue posible comunicarse con el servicio de pedidos. Verifique que la API esté en ejecución.";

    private const string UnexpectedError =
        "El servicio de pedidos no pudo procesar la solicitud. Intente nuevamente más tarde.";

    public async Task<ApiResult<OrderModel>> CreateOrderAsync(OrderFormModel form, CancellationToken cancellationToken = default)
    {
        var request = OrderFormMapper.ToApiRequest(form);

        try
        {
            using var response = await httpClient.PostAsJsonAsync("api/pedidos", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<OrderModel>.Fail(await ReadErrorsAsync(response, cancellationToken));
            }

            var order = await response.Content.ReadFromJsonAsync<OrderModel>(cancellationToken);
            return order is null
                ? ApiResult<OrderModel>.Fail(UnexpectedError)
                : ApiResult<OrderModel>.Ok(order);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogError(ex, "No se pudo crear el pedido por un error de comunicación con la API.");
            return ApiResult<OrderModel>.Fail(ConnectionError);
        }
    }

    public async Task<ApiResult<PagedResultModel<OrderModel>>> GetOrdersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync($"api/pedidos?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<PagedResultModel<OrderModel>>.Fail(await ReadErrorsAsync(response, cancellationToken));
            }

            var page = await response.Content.ReadFromJsonAsync<PagedResultModel<OrderModel>>(cancellationToken);
            return page is null
                ? ApiResult<PagedResultModel<OrderModel>>.Fail(UnexpectedError)
                : ApiResult<PagedResultModel<OrderModel>>.Ok(page);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogError(ex, "No se pudo obtener el listado de pedidos por un error de comunicación con la API.");
            return ApiResult<PagedResultModel<OrderModel>>.Fail(ConnectionError);
        }
    }

    private async Task<string[]> ReadErrorsAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        logger.LogWarning("La API respondió con el código {StatusCode}.", (int)response.StatusCode);

        if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
        {
            return new[] { UnexpectedError };
        }

        try
        {
            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

            if (document.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
            {
                var messages = errors.EnumerateArray()
                    .Select(error => error.GetString())
                    .OfType<string>()
                    .ToArray();

                if (messages.Length > 0)
                {
                    return messages;
                }
            }
        }
        catch (JsonException)
        {
        }

        return new[] { "Los datos enviados no son válidos." };
    }
}

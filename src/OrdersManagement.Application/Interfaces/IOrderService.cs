using OrdersManagement.Application.DTOs;

namespace OrdersManagement.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);

    Task<PagedResult<OrderResponse>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task ConfirmAsync(long orderId, CancellationToken cancellationToken);
}

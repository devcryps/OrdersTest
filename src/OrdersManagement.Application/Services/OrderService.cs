using Microsoft.Extensions.Logging;
using OrdersManagement.Application.DTOs;
using OrdersManagement.Application.Events;
using OrdersManagement.Application.Exceptions;
using OrdersManagement.Application.Interfaces;
using OrdersManagement.Application.Mappings;
using OrdersManagement.Domain.Enums;

namespace OrdersManagement.Application.Services;

public class OrderService(
    IOrderRepository repository,
    IOrderEventPublisher publisher,
    IOrderCache cache,
    ILogger<OrderService> logger) : IOrderService
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = OrderMapper.ToEntity(request, DateTime.UtcNow);

        await repository.AddAsync(order, cancellationToken);
        logger.LogInformation("Pedido {OrderId} creado para el cliente con {ItemCount} ítems y total {Total}.",
            order.Id, order.Items.Count, order.Total);

        await publisher.PublishOrderCreatedAsync(new OrderCreated(order.Id, order.CreatedAt), cancellationToken);
        logger.LogInformation("Evento OrderCreated publicado para el pedido {OrderId}.", order.Id);

        await cache.InvalidateListAsync(cancellationToken);

        return OrderMapper.ToResponse(order);
    }

    public async Task<PagedResult<OrderResponse>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        ValidatePagination(pageNumber, pageSize);

        var cached = await cache.GetPageAsync(pageNumber, pageSize, cancellationToken);
        if (cached is not null)
        {
            logger.LogDebug("Listado de pedidos obtenido desde caché (página {PageNumber}, tamaño {PageSize}).",
                pageNumber, pageSize);
            return cached;
        }

        var totalCount = await repository.CountAsync(cancellationToken);
        var orders = await repository.GetPagedAsync(pageNumber, pageSize, cancellationToken);

        var page = new PagedResult<OrderResponse>
        {
            Items = orders.Select(OrderMapper.ToResponse).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        await cache.SetPageAsync(pageNumber, pageSize, page, cancellationToken);

        return page;
    }

    public async Task ConfirmAsync(long orderId, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new OrderNotFoundException(orderId);

        if (!order.Confirm())
        {
            logger.LogInformation("El pedido {OrderId} ya estaba confirmado; no se realizan cambios.", orderId);
            return;
        }

        await repository.UpdateStatusAsync(orderId, OrderStatus.Confirmed, cancellationToken);
        await cache.InvalidateListAsync(cancellationToken);

        logger.LogInformation("Pedido {OrderId} confirmado.", orderId);
    }

    private static void ValidatePagination(int pageNumber, int pageSize)
    {
        var errors = new List<string>();

        if (pageNumber < 1)
        {
            errors.Add("El número de página debe ser mayor o igual a 1.");
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            errors.Add($"El tamaño de página debe estar entre 1 y {MaxPageSize}.");
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors.ToArray());
        }
    }
}

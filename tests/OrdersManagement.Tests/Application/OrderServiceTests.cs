using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrdersManagement.Application.DTOs;
using OrdersManagement.Application.Exceptions;
using OrdersManagement.Application.Services;
using OrdersManagement.Domain.Enums;
using OrdersManagement.Domain.Exceptions;
using OrdersManagement.Infrastructure.Caching;
using OrdersManagement.Infrastructure.Caching.InMemory;
using OrdersManagement.Tests.Fakes;

namespace OrdersManagement.Tests.Application;

public class OrderServiceTests
{
    private readonly CountingOrderRepository _repository = new();
    private readonly FakeOrderEventPublisher _publisher = new();
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        var cache = new MemoryOrderCache(
            new MemoryCache(new MemoryCacheOptions()),
            Options.Create(new CacheOptions { ListExpirationSeconds = 60 }));

        _service = new OrderService(_repository, _publisher, cache, NullLogger<OrderService>.Instance);
    }

    private static CreateOrderRequest ValidRequest(string customer = "Ana") => new()
    {
        Customer = customer,
        Total = 25m,
        Items = new List<CreateOrderItemRequest>
        {
            new() { Description = "A", Quantity = 2, UnitPrice = 10.5m },
            new() { Description = "B", Quantity = 1, UnitPrice = 4m }
        }
    };

    [Fact]
    public async Task Create_WithValidRequest_SavesOrderAsPendingAndPublishesEvent()
    {
        var response = await _service.CreateAsync(ValidRequest(), CancellationToken.None);

        Assert.True(response.Id > 0);
        Assert.Equal("Pending", response.Status);
        Assert.Equal(2, response.Items.Count);
        Assert.Equal(response.Id, Assert.Single(_publisher.Published).OrderId);
        Assert.NotNull(await _repository.GetByIdAsync(response.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Create_WithInvalidRequest_ThrowsAndDoesNotPublish()
    {
        await Assert.ThrowsAsync<DomainValidationException>(() =>
            _service.CreateAsync(ValidRequest(customer: ""), CancellationToken.None));

        Assert.Empty(_publisher.Published);
        Assert.Equal(0, await _repository.CountAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Confirm_PendingOrder_MarksItAsConfirmed()
    {
        var created = await _service.CreateAsync(ValidRequest(), CancellationToken.None);

        await _service.ConfirmAsync(created.Id, CancellationToken.None);

        var order = await _repository.GetByIdAsync(created.Id, CancellationToken.None);
        Assert.Equal(OrderStatus.Confirmed, order!.Status);
        Assert.Equal(1, _repository.UpdateStatusCalls);
    }

    [Fact]
    public async Task Confirm_RepeatedTwice_IsIdempotent()
    {
        var created = await _service.CreateAsync(ValidRequest(), CancellationToken.None);

        await _service.ConfirmAsync(created.Id, CancellationToken.None);
        await _service.ConfirmAsync(created.Id, CancellationToken.None);

        Assert.Equal(1, _repository.UpdateStatusCalls);
    }

    [Fact]
    public async Task Confirm_UnknownOrder_Throws()
    {
        await Assert.ThrowsAsync<OrderNotFoundException>(() => _service.ConfirmAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task GetPaged_ReturnsNewestFirstWithPaginationData()
    {
        for (var i = 1; i <= 5; i++)
        {
            await _service.CreateAsync(ValidRequest($"Cliente {i}"), CancellationToken.None);
        }

        var page = await _service.GetPagedAsync(1, 2, CancellationToken.None);

        Assert.Equal(new[] { "Cliente 5", "Cliente 4" }, page.Items.Select(o => o.Customer));
        Assert.Equal(5, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, OrderService.MaxPageSize + 1)]
    public async Task GetPaged_WithInvalidPagination_Throws(int pageNumber, int pageSize)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GetPagedAsync(pageNumber, pageSize, CancellationToken.None));
    }

    [Fact]
    public async Task GetPaged_SecondCallUsesCache()
    {
        await _service.GetPagedAsync(1, 10, CancellationToken.None);
        await _service.GetPagedAsync(1, 10, CancellationToken.None);

        Assert.Equal(1, _repository.CountCalls);
    }

    [Fact]
    public async Task Create_InvalidatesCachedList()
    {
        var before = await _service.GetPagedAsync(1, 10, CancellationToken.None);

        await _service.CreateAsync(ValidRequest(), CancellationToken.None);
        var after = await _service.GetPagedAsync(1, 10, CancellationToken.None);

        Assert.Equal(0, before.TotalCount);
        Assert.Equal(1, after.TotalCount);
    }

    [Fact]
    public async Task Confirm_InvalidatesCachedList()
    {
        var created = await _service.CreateAsync(ValidRequest(), CancellationToken.None);
        var pending = await _service.GetPagedAsync(1, 10, CancellationToken.None);

        await _service.ConfirmAsync(created.Id, CancellationToken.None);
        var confirmed = await _service.GetPagedAsync(1, 10, CancellationToken.None);

        Assert.Equal("Pending", pending.Items[0].Status);
        Assert.Equal("Confirmed", confirmed.Items[0].Status);
    }
}


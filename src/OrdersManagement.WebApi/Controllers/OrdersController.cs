using Microsoft.AspNetCore.Mvc;
using OrdersManagement.Application.DTOs;
using OrdersManagement.Application.Interfaces;
using OrdersManagement.Application.Services;

namespace OrdersManagement.WebApi.Controllers;

[ApiController]
[Route("api/pedidos")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await orderService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetPaged), new { pageNumber = 1, pageSize = OrderService.DefaultPageSize }, order);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = OrderService.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var page = await orderService.GetPagedAsync(pageNumber, pageSize, cancellationToken);

        return Ok(page);
    }
}

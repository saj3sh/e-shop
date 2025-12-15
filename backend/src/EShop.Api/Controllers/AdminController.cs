using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShop.Application.Orders;

namespace EShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        [FromServices] GetOrdersQueryHandler handler,
        [FromQuery] string? status = null,
        [FromQuery] string? trackingNumber = null,
        CancellationToken ct = default)
    {
        var query = new GetOrdersQuery(status, trackingNumber);
        var result = await handler.HandleAsync(query, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("orders/{id:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateStatusRequest request,
        [FromServices] UpdateOrderStatusCommandHandler handler,
        CancellationToken ct)
    {
        var command = new UpdateOrderStatusCommand(id, request.Status);
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }
}

public record UpdateStatusRequest(string Status);

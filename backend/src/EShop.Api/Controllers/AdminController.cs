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
    public async Task<IActionResult> GetIncompleteOrders(
        [FromServices] GetIncompleteOrdersQueryHandler handler,
        CancellationToken ct)
    {
        var query = new GetIncompleteOrdersQuery();
        var result = await handler.HandleAsync(query, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("orders/{id:guid}/complete")]
    public async Task<IActionResult> CompleteOrder(
        Guid id,
        [FromServices] CompleteOrderCommandHandler handler,
        CancellationToken ct)
    {
        var command = new CompleteOrderCommand(id);
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }
}

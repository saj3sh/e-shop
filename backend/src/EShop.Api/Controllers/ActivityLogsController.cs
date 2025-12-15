using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShop.Application.ActivityLogs;

namespace EShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class ActivityLogsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActivityLogs(
        [FromServices] GetActivityLogsQueryHandler handler,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] string? userId = null,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var query = new GetActivityLogsQuery(entityType, limit);
        var result = await handler.HandleAsync(query, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}

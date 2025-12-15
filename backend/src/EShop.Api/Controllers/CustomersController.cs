using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShop.Application.Customers;
using System.Security.Claims;

namespace EShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(
        [FromServices] GetCustomerByIdQueryHandler handler,
        CancellationToken ct)
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (customerIdClaim == null)
            return Unauthorized();

        var query = new GetCustomerByIdQuery(Guid.Parse(customerIdClaim));
        var result = await handler.HandleAsync(query, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateCustomerRequest request,
        [FromServices] UpdateCustomerCommandHandler handler,
        CancellationToken ct)
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (customerIdClaim == null)
            return Unauthorized();

        var command = new UpdateCustomerCommand(
            Guid.Parse(customerIdClaim),
            request.FirstName,
            request.LastName,
            request.Phone
        );

        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }
}

public record UpdateCustomerRequest(string FirstName, string LastName, string Phone);

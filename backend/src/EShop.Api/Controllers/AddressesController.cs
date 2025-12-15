using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShop.Application.Addresses;

namespace EShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AddressesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCustomerAddresses(
        [FromServices] GetCustomerAddressesQueryHandler handler,
        CancellationToken ct)
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (customerIdClaim == null)
            return Unauthorized();

        var customerId = Guid.Parse(customerIdClaim);
        var query = new GetCustomerAddressesQuery(customerId);
        var result = await handler.HandleAsync(query, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress(
        [FromBody] CreateAddressRequest request,
        [FromServices] CreateAddressCommandHandler handler,
        CancellationToken ct)
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (customerIdClaim == null)
            return Unauthorized();

        var customerId = Guid.Parse(customerIdClaim);
        var command = new CreateAddressCommand(
            customerId,
            request.Line1,
            request.City,
            request.Country,
            request.Type
        );

        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { id = result.Value });
    }

    [HttpPut("{addressId}/set-default")]
    public async Task<IActionResult> SetDefaultAddress(
        Guid addressId,
        [FromBody] SetDefaultAddressRequest request,
        [FromServices] SetDefaultAddressCommandHandler handler,
        CancellationToken ct)
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (customerIdClaim == null)
            return Unauthorized();

        var customerId = Guid.Parse(customerIdClaim);
        var command = new SetDefaultAddressCommand(customerId, addressId, request.AddressType);
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpDelete("{addressId}")]
    public async Task<IActionResult> DeleteAddress(
        Guid addressId,
        [FromServices] DeleteAddressCommandHandler handler,
        CancellationToken ct)
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (customerIdClaim == null)
            return Unauthorized();

        var customerId = Guid.Parse(customerIdClaim);
        var command = new DeleteAddressCommand(customerId, addressId);
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }
}

public record CreateAddressRequest(string Line1, string City, string Country, string Type);
public record SetDefaultAddressRequest(string AddressType);

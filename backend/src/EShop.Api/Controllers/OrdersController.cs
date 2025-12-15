using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShop.Application.Orders;
using System.Security.Claims;

namespace EShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Checkout(
        [FromBody] CheckoutRequest request,
        [FromServices] CheckoutOrderCommandHandler handler,
        CancellationToken ct)
    {
        var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (customerIdClaim == null)
            return Unauthorized();

        var command = new CheckoutOrderCommand(
            Guid.Parse(customerIdClaim),
            request.Items.Select(i => new OrderItemDto(i.ProductId, i.Quantity)).ToList(),
            request.ShippingAddressId,
            request.BillingAddressId,
            request.ShippingCountry
        );

        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyOrders(
        [FromServices] GetCustomerOrdersQueryHandler handler,
        CancellationToken ct)
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (customerIdClaim == null)
            return Unauthorized();

        var query = new GetCustomerOrdersQuery(Guid.Parse(customerIdClaim));
        var result = await handler.HandleAsync(query, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}

public record CheckoutRequest(
    List<CheckoutItemRequest> Items,
    Guid ShippingAddressId,
    Guid BillingAddressId,
    string ShippingCountry
);

public record CheckoutItemRequest(Guid ProductId, int Quantity);

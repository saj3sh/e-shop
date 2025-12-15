using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShop.Domain.Customers;
using EShop.Infrastructure.Persistence;

namespace EShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AddressesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request, CancellationToken ct)
    {
        var address = new Address(
            Guid.NewGuid(),
            request.Line1,
            request.City,
            request.Country,
            Enum.Parse<AddressType>(request.Type),
            null
        );

        await _context.Addresses.AddAsync(address, ct);
        await _context.SaveChangesAsync(ct);

        return Ok(new { id = address.Id });
    }
}

public record CreateAddressRequest(string Line1, string City, string Country, string Type);

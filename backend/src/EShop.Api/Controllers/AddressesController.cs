using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShop.Domain.Customers;
using EShop.Application.Common;

namespace EShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IUnitOfWork _unitOfWork;

    public AddressesController(ICustomerRepository customerRepo, IUnitOfWork unitOfWork)
    {
        _customerRepo = customerRepo;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomerAddresses(CancellationToken ct)
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (customerIdClaim == null)
            return Unauthorized();

        var customer = await _customerRepo.GetByIdAsync(new CustomerId(Guid.Parse(customerIdClaim)), ct);
        if (customer == null)
            return NotFound("Customer not found");

        return Ok(new
        {
            defaultShippingAddressId = customer.DefaultShippingAddressId,
            defaultBillingAddressId = customer.DefaultBillingAddressId
        });
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

        _customerRepo.AddAddress(address);
        await _unitOfWork.SaveChangesAsync(ct);

        return Ok(new { id = address.Id });
    }
}

public record CreateAddressRequest(string Line1, string City, string Country, string Type);

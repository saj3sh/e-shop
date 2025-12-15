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

        var customerId = Guid.Parse(customerIdClaim);
        var customer = await _customerRepo.GetByIdAsync(new CustomerId(customerId), ct);
        if (customer == null)
            return NotFound("Customer not found");

        var addresses = await _customerRepo.GetCustomerAddressesAsync(customerId, ct);

        return Ok(new
        {
            addresses = addresses.Select(a => new
            {
                id = a.Id,
                line1 = a.Line1,
                city = a.City,
                country = a.Country,
                type = a.Type.ToString()
            }),
            defaultShippingAddressId = customer.DefaultShippingAddressId,
            defaultBillingAddressId = customer.DefaultBillingAddressId
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request, CancellationToken ct)
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (customerIdClaim == null)
            return Unauthorized();

        var customerId = Guid.Parse(customerIdClaim);

        var address = new Address(
            Guid.NewGuid(),
            request.Line1,
            request.City,
            request.Country,
            Enum.Parse<AddressType>(request.Type),
            customerId
        );

        _customerRepo.AddAddress(address);
        await _unitOfWork.SaveChangesAsync(ct);

        return Ok(new { id = address.Id });
    }

    [HttpPut("{addressId}/set-default")]
    public async Task<IActionResult> SetDefaultAddress(Guid addressId, [FromBody] SetDefaultAddressRequest request, CancellationToken ct)
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (customerIdClaim == null)
            return Unauthorized();

        var customerId = new CustomerId(Guid.Parse(customerIdClaim));
        var customer = await _customerRepo.GetByIdAsync(customerId, ct);
        if (customer == null)
            return NotFound("Customer not found");

        var address = await _customerRepo.GetAddressAsync(addressId, ct);
        if (address == null || address.CustomerId != customerId.Value)
            return NotFound("Address not found");

        if (request.AddressType == "Shipping")
            customer.SetDefaultShippingAddress(addressId);
        else if (request.AddressType == "Billing")
            customer.SetDefaultBillingAddress(addressId);
        else
            return BadRequest("Invalid address type");

        _customerRepo.Update(customer);
        await _unitOfWork.SaveChangesAsync(ct);

        return Ok();
    }
}

public record CreateAddressRequest(string Line1, string City, string Country, string Type);
public record SetDefaultAddressRequest(string AddressType);

using EShop.Domain.Customers;
using EShop.Domain.Addresses;
using EShop.Application.Common;

namespace EShop.Application.Addresses;

public class GetCustomerAddressesQueryHandler : IQueryHandler<GetCustomerAddressesQuery, Result<CustomerAddressesDto>>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IAddressRepository _addressRepo;

    public GetCustomerAddressesQueryHandler(ICustomerRepository customerRepo, IAddressRepository addressRepo)
    {
        _customerRepo = customerRepo;
        _addressRepo = addressRepo;
    }

    public async Task<Result<CustomerAddressesDto>> HandleAsync(GetCustomerAddressesQuery query, CancellationToken ct = default)
    {
        var customer = await _customerRepo.GetByIdAsync(new CustomerId(query.CustomerId), ct);
        if (customer == null)
            return Result<CustomerAddressesDto>.Failure("Customer not found");

        var addresses = await _addressRepo.GetByCustomerIdAsync(query.CustomerId, ct);

        var addressDtos = addresses.Select(a => new AddressDto(
            a.Id,
            a.Line1,
            a.City,
            a.Country,
            a.Type.ToString()
        )).ToList();

        var result = new CustomerAddressesDto(
            addressDtos,
            customer.DefaultShippingAddressId,
            customer.DefaultBillingAddressId
        );

        return Result<CustomerAddressesDto>.Success(result);
    }
}

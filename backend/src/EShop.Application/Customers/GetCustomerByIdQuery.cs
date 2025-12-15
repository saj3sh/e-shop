using EShop.Domain.Customers;
using EShop.Application.Common;

namespace EShop.Application.Customers;

public record GetCustomerByIdQuery(Guid Id) : IQuery<Result<CustomerDto?>>;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    Guid? DefaultShippingAddressId,
    Guid? DefaultBillingAddressId
)
{
    public static explicit operator CustomerDto(Customer c) => new(
        c.Id.Value,
        c.FirstName,
        c.LastName,
        c.Email.Value,
        c.Phone.Value,
        c.DefaultShippingAddressId,
        c.DefaultBillingAddressId
    );
}

public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, Result<CustomerDto?>>
{
    private readonly ICustomerRepository _customerRepo;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepo)
    {
        _customerRepo = customerRepo;
    }

    public async Task<Result<CustomerDto?>> HandleAsync(GetCustomerByIdQuery query, CancellationToken ct = default)
    {
        var customer = await _customerRepo.GetByIdAsync(new CustomerId(query.Id), ct);

        if (customer == null)
            return Result<CustomerDto?>.Failure("customer not found");

        return Result<CustomerDto?>.Success((CustomerDto)customer);
    }
}

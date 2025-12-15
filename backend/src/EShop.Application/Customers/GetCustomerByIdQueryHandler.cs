using EShop.Domain.Customers;
using EShop.Domain.Auth;
using EShop.Application.Common;

namespace EShop.Application.Customers;

public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, Result<CustomerDto?>>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IUserAccountRepository _userAccountRepo;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepo, IUserAccountRepository userAccountRepo)
    {
        _customerRepo = customerRepo;
        _userAccountRepo = userAccountRepo;
    }

    public async Task<Result<CustomerDto?>> HandleAsync(GetCustomerByIdQuery query, CancellationToken ct = default)
    {
        var customer = await _customerRepo.GetByIdAsync(new CustomerId(query.Id), ct);

        if (customer == null)
            return Result<CustomerDto?>.Failure("customer not found");

        // Get email from UserAccount
        var userAccount = await _userAccountRepo.GetByCustomerIdAsync(customer.Id, ct);
        var email = userAccount?.Email.Value ?? string.Empty;

        var dto = new CustomerDto(
            customer.Id.Value,
            customer.FirstName,
            customer.LastName,
            email,
            customer.Phone.Value,
            customer.DefaultShippingAddressId,
            customer.DefaultBillingAddressId
        );

        return Result<CustomerDto?>.Success(dto);
    }
}

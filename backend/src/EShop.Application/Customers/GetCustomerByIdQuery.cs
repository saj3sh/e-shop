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
);

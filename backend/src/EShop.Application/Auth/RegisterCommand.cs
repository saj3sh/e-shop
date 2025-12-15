using EShop.Application.Common;

namespace EShop.Application.Auth;

public record RegisterCommand(
    string Email,
    string FirstName,
    string LastName,
    string Phone,
    string ShippingAddress,
    string ShippingCity,
    string ShippingCountryCode,
    string BillingAddress,
    string BillingCity,
    string BillingCountryCode
) : ICommand<Result<RegisterResult>>;

public record RegisterResult(Guid UserId, string Email);

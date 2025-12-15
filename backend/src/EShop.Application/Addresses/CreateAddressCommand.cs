using EShop.Application.Common;

namespace EShop.Application.Addresses;

public record CreateAddressCommand(
    Guid CustomerId,
    string Line1,
    string City,
    string Country,
    string Type
) : ICommand<Result<Guid>>;

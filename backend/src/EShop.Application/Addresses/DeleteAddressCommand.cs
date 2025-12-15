using EShop.Application.Common;

namespace EShop.Application.Addresses;

public record DeleteAddressCommand(
    Guid CustomerId,
    Guid AddressId
) : ICommand<Result>;

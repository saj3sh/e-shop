using EShop.Application.Common;

namespace EShop.Application.Addresses;

public record SetDefaultAddressCommand(
    Guid CustomerId,
    Guid AddressId,
    string AddressType
) : ICommand<Result>;

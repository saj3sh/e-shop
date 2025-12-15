namespace EShop.Application.Addresses;

public record CustomerAddressesDto(
    List<AddressDto> Addresses,
    Guid? DefaultShippingAddressId,
    Guid? DefaultBillingAddressId
);

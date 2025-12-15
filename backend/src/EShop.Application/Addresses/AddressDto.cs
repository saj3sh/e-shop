namespace EShop.Application.Addresses;

public record AddressDto(
    Guid Id,
    string Line1,
    string City,
    string Country,
    string Type
);

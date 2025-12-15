using EShop.Application.Common;

namespace EShop.Application.Addresses;

public record GetCustomerAddressesQuery(Guid CustomerId) : IQuery<Result<CustomerAddressesDto>>;

using EShop.Application.Common;

namespace EShop.Application.Orders;

public record CheckoutOrderCommand(
    Guid CustomerId,
    List<OrderItemDto> Items,
    Guid? ShippingAddressId = null,
    Guid? BillingAddressId = null,
    string? CardNumber = null,
    string? CardType = null
) : ICommand<Result<OrderDto>>;

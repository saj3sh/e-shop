using EShop.Domain.Orders;
using EShop.Domain.Products;
using EShop.Domain.Customers;
using EShop.Application.Common;

namespace EShop.Application.Orders;

public record CheckoutOrderCommand(
    Guid CustomerId,
    List<OrderItemDto> Items,
    Guid ShippingAddressId,
    Guid BillingAddressId,
    string ShippingCountry,
    string? CardNumber = null,
    string? CardType = null
) : ICommand<Result<OrderDto>>;

public record OrderItemDto(Guid ProductId, int Quantity);

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    string TrackingNumber,
    DateTime PurchaseDate,
    DateTime? EstimatedDelivery,
    decimal Total,
    string? PaymentCard,
    List<OrderItemDetailDto> Items
)
{
    public static explicit operator OrderDto(Order o) => new(
        o.Id.Value,
        o.CustomerId.Value,
        o.Status.ToString(),
        o.TrackingNumber.Value,
        o.PurchaseDate,
        o.EstimatedDelivery,
        o.Total.Amount,
        o.PaymentCard?.ToString(),
        [.. o.Items.Select(i => (OrderItemDetailDto)i)]
    );
}

public record OrderItemDetailDto(Guid ProductId, int Quantity, decimal UnitPrice, decimal TotalPrice)
{
    public static explicit operator OrderItemDetailDto(OrderItem oi) => new(
        oi.ProductId.Value,
        oi.Quantity,
        oi.UnitPrice.Amount,
        oi.TotalPrice.Amount
    );
}

public class CheckoutOrderCommandHandler : ICommandHandler<CheckoutOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly ICustomerRepository _customerRepo;

    public CheckoutOrderCommandHandler(
        IOrderRepository orderRepo,
        IProductRepository productRepo,
        ICustomerRepository customerRepo)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _customerRepo = customerRepo;
    }

    public async Task<Result<OrderDto>> HandleAsync(CheckoutOrderCommand command, CancellationToken ct = default)
    {
        try
        {
            if (command.Items.Count == 0)
                return Result<OrderDto>.Failure("Cart is empty");

            var customerId = new CustomerId(command.CustomerId);
            var trackingNumber = TrackingNumber.Generate(command.ShippingCountry);
            var paymentCard = PaymentCard.CreateMasked(command.CardNumber, command.CardType);

            var order = new Order(
                OrderId.New(),
                customerId,
                trackingNumber,
                command.ShippingAddressId,
                command.BillingAddressId,
                DateTime.UtcNow,
                paymentCard
            );

            foreach (var item in command.Items)
            {
                var product = await _productRepo.GetByIdAsync(new ProductId(item.ProductId), ct);
                if (product == null)
                    return Result<OrderDto>.Failure($"product {item.ProductId} not found");

                var orderItem = new OrderItem(
                    Guid.NewGuid(),
                    order.Id,
                    product.Id,
                    item.Quantity,
                    product.Price
                );

                order.AddItem(orderItem);
            }

            await _orderRepo.AddAsync(order, ct);

            return Result<OrderDto>.Success((OrderDto)order);
        }
        catch (Exception ex)
        {
            return Result<OrderDto>.Failure($"checkout failed: {ex.Message}");
        }
    }
}

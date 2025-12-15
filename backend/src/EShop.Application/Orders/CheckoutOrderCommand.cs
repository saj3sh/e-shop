using EShop.Domain.Orders;
using EShop.Domain.Products;
using EShop.Domain.Customers;
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
    public static OrderDto FromOrder(Order o, Dictionary<Guid, string> productNames) => new(
        o.Id.Value,
        o.CustomerId.Value,
        o.Status.ToString(),
        o.TrackingNumber.Value,
        o.PurchaseDate,
        o.EstimatedDelivery,
        o.Total.Amount,
        o.PaymentCard?.ToString(),
        o.Items.Select(i => new OrderItemDetailDto(
            i.ProductId.Value,
            productNames.GetValueOrDefault(i.ProductId.Value, "Unknown Product"),
            i.Quantity,
            i.UnitPrice.Amount,
            i.TotalPrice.Amount
        )).ToList()
    );
}

public record OrderItemDetailDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal TotalPrice);

public class CheckoutOrderCommandHandler : ICommandHandler<CheckoutOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutOrderCommandHandler(
        IOrderRepository orderRepo,
        IProductRepository productRepo,
        ICustomerRepository customerRepo,
        IUnitOfWork unitOfWork)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _customerRepo = customerRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> HandleAsync(CheckoutOrderCommand command, CancellationToken ct = default)
    {
        try
        {
            if (command.Items.Count == 0)
                return Result<OrderDto>.Failure("Cart is empty");

            // Begin transaction for order checkout
            await _unitOfWork.BeginTransactionAsync(ct);

            var customerId = new CustomerId(command.CustomerId);

            // Get customer to use default addresses if not specified
            var customer = await _customerRepo.GetByIdAsync(customerId, ct);
            if (customer == null)
                return Result<OrderDto>.Failure("Customer not found");

            // Use provided addresses or fall back to customer defaults
            var shippingAddressId = command.ShippingAddressId ?? customer.DefaultShippingAddressId;
            var billingAddressId = command.BillingAddressId ?? customer.DefaultBillingAddressId;

            if (shippingAddressId == null || billingAddressId == null)
                return Result<OrderDto>.Failure("Customer must have default addresses set");

            // Get shipping address to infer country code for tracking number
            var shippingAddress = await _customerRepo.GetAddressAsync(shippingAddressId.Value, ct);
            if (shippingAddress == null)
                return Result<OrderDto>.Failure("Shipping address not found");

            var trackingNumber = TrackingNumber.Generate(shippingAddress.Country);
            var paymentCard = PaymentCard.CreateMasked(command.CardNumber, command.CardType);

            var order = new Order(
                OrderId.New(),
                customerId,
                trackingNumber,
                shippingAddressId.Value,
                billingAddressId.Value,
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

            _orderRepo.Add(order);
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            // Build product names dictionary for DTO
            var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _productRepo.GetByIdsAsync(productIds, ct);
            var productNames = products.ToDictionary(p => p.Id.Value, p => p.Name);

            return Result<OrderDto>.Success(OrderDto.FromOrder(order, productNames));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            return Result<OrderDto>.Failure($"checkout failed: {ex.Message}");
        }
    }
}

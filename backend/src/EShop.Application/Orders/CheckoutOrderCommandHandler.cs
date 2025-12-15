using EShop.Domain.Orders;
using EShop.Domain.Products;
using EShop.Domain.Customers;
using EShop.Domain.Addresses;
using EShop.Application.Common;

namespace EShop.Application.Orders;

public class CheckoutOrderCommandHandler : ICommandHandler<CheckoutOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IAddressRepository _addressRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutOrderCommandHandler(
        IOrderRepository orderRepo,
        IProductRepository productRepo,
        ICustomerRepository customerRepo,
        IAddressRepository addressRepo,
        IUnitOfWork unitOfWork)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _customerRepo = customerRepo;
        _addressRepo = addressRepo;
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
            var shippingAddress = await _addressRepo.GetByIdAsync(shippingAddressId.Value, ct);
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

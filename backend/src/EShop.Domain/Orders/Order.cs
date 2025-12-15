namespace EShop.Domain.Orders;

/// <summary>
/// order aggregate, tracks customer purchases
/// </summary>
public class Order : Common.AggregateRoot
{
    public OrderId Id { get; private set; } = null!;
    public Customers.CustomerId CustomerId { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public TrackingNumber TrackingNumber { get; private set; } = null!;
    public DateTime PurchaseDate { get; private set; }
    public DateTime? EstimatedDelivery { get; private set; }
    public Guid ShippingAddressId { get; private set; }
    public Guid BillingAddressId { get; private set; }
    public PaymentCard? PaymentCard { get; private set; }
    public Products.Money Total { get; private set; } = null!;

    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public Order(
        OrderId id,
        Customers.CustomerId customerId,
        TrackingNumber trackingNumber,
        Guid shippingAddressId,
        Guid billingAddressId,
        DateTime purchaseDate,
        PaymentCard? paymentCard = null,
        DateTime? estimatedDelivery = null)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        TrackingNumber = trackingNumber;
        ShippingAddressId = shippingAddressId;
        BillingAddressId = billingAddressId;
        PurchaseDate = purchaseDate;
        PaymentCard = paymentCard;
        EstimatedDelivery = estimatedDelivery ?? purchaseDate.AddDays(7);
        Total = Products.Money.Create(0);

        RaiseDomainEvent(new OrderPlaced(Id, CustomerId, DateTime.UtcNow));
    }

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
        RecalculateTotal();
    }

    public void MarkAsCompleted()
    {
        if (Status == OrderStatus.Completed)
            return;

        Status = OrderStatus.Completed;
        RaiseDomainEvent(new OrderCompleted(Id, DateTime.UtcNow));
    }

    private void RecalculateTotal()
    {
        Total = _items
            .Select(i => i.TotalPrice)
            .Aggregate(Products.Money.Create(0), (acc, price) => acc.Add(price));
    }
}

public record OrderId(Guid Value) : Common.EntityId<Guid>(Value)
{
    public static OrderId New() => new(Guid.NewGuid());
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Completed,
    Cancelled
}

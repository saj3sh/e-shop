namespace EShop.Domain.Orders;

/// <summary>
/// order line item entity
/// </summary>
public class OrderItem : Common.Entity<Guid>
{
    public OrderId OrderId { get; private set; } = null!;
    public Products.ProductId ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Products.Money UnitPrice { get; private set; }
    public Products.Money TotalPrice { get; private set; }

    private OrderItem() { }

    public OrderItem(Guid id, OrderId orderId, Products.ProductId productId, int quantity, Products.Money unitPrice)
        : base(id)
    {
        if (quantity <= 0)
            throw new ArgumentException("quantity must be positive");
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = unitPrice.Multiply(quantity);
    }
}

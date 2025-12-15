namespace EShop.Domain.Orders;

/// <summary>
/// order line item entity
/// </summary>
public class OrderItem
{
    public Guid Id { get; private set; }
    public OrderId OrderId { get; private set; }
    public Products.ProductId ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Products.Money UnitPrice { get; private set; }
    public Products.Money TotalPrice { get; private set; }

    private OrderItem() { }

    public OrderItem(Guid id, OrderId orderId, Products.ProductId productId, int quantity, Products.Money unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("quantity must be positive");

        Id = id;
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = unitPrice.Multiply(quantity);
    }
}

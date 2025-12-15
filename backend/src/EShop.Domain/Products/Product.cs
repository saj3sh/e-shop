namespace EShop.Domain.Products;

/// <summary>
/// product aggregate, simple catalog item
/// </summary>
public class Product : Common.AggregateRoot
{
    public ProductId Id { get; private set; }
    public string Name { get; private set; }
    public Money Price { get; private set; }
    public Sku Sku { get; private set; }
    public string ManufacturedFrom { get; private set; }
    public string ShippedFrom { get; private set; }

    private Product() { }

    public Product(ProductId id, string name, Money price, Sku sku, string manufacturedFrom, string shippedFrom)
    {
        Id = id;
        Name = name;
        Price = price;
        Sku = sku;
        ManufacturedFrom = manufacturedFrom;
        ShippedFrom = shippedFrom;
    }

    public void UpdatePrice(Money newPrice)
    {
        Price = newPrice;
    }
}

public record ProductId(Guid Value) : Common.EntityId<Guid>(Value)
{
    public static ProductId New() => new(Guid.NewGuid());
}

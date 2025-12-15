namespace EShop.Domain.Customers;

/// <summary>
/// address entity, can be shipping or billing
/// </summary>
public class Address
{
    public Guid Id { get; private set; }
    public string Line1 { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }
    public AddressType Type { get; private set; }
    public Guid? CustomerId { get; private set; }

    private Address() { }

    public Address(Guid id, string line1, string city, string country, AddressType type, Guid? customerId = null)
    {
        Id = id;
        Line1 = line1;
        City = city;
        Country = country;
        Type = type;
        CustomerId = customerId;
    }

    public void UpdateType(AddressType type)
    {
        Type = type;
    }
}

public enum AddressType
{
    Shipping,
    Billing,
    Both
}

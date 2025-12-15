namespace EShop.Domain.Customers;

/// <summary>
/// customer aggregate root, holds personal info and addresses
/// </summary>
public class Customer : Common.AggregateRoot
{
    public CustomerId Id { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public Phone Phone { get; private set; } = null!;
    public Guid? DefaultShippingAddressId { get; private set; }
    public Guid? DefaultBillingAddressId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Customer() { }

    public Customer(CustomerId id, string firstName, string lastName, Phone phone)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string firstName, string lastName, Phone phone)
    {
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
    }

    public void SetDefaultShippingAddress(Guid addressId)
    {
        DefaultShippingAddressId = addressId;
    }

    public void SetDefaultBillingAddress(Guid addressId)
    {
        DefaultBillingAddressId = addressId;
    }
}

public record CustomerId(Guid Value) : Common.EntityId<Guid>(Value)
{
    public static CustomerId New() => new(Guid.NewGuid());
}

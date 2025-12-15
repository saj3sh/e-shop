namespace EShop.Domain.Customers;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken ct = default);
    Task<Address?> GetAddressAsync(Guid addressId, CancellationToken ct = default);
    void Add(Customer customer);
    void AddAddress(Address address);
    void Update(Customer customer);
}

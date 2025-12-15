namespace EShop.Domain.Customers;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken ct = default);
    Task<Customer?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<Address?> GetAddressAsync(Guid addressId, CancellationToken ct = default);
    Task AddAsync(Customer customer, CancellationToken ct = default);
    Task AddAddressAsync(Address address, CancellationToken ct = default);
    Task UpdateAsync(Customer customer, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(Email email, CancellationToken ct = default);
}

namespace EShop.Domain.Addresses;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid addressId, CancellationToken ct = default);
    Task<List<Address>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    void Add(Address address);
    void Update(Address address);
    void Delete(Address address);
}

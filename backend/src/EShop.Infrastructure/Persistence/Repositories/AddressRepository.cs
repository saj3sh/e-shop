using Microsoft.EntityFrameworkCore;
using EShop.Domain.Addresses;

namespace EShop.Infrastructure.Persistence.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly AppDbContext _context;

    public AddressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Address?> GetByIdAsync(Guid addressId, CancellationToken ct = default)
    {
        return await _context.Addresses.FindAsync([addressId], ct);
    }

    public async Task<List<Address>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Addresses
            .Where(a => a.CustomerId == customerId)
            .ToListAsync(ct);
    }

    public void Add(Address address)
    {
        _context.Addresses.Add(address);
    }

    public void Update(Address address)
    {
        _context.Addresses.Update(address);
    }

    public void Delete(Address address)
    {
        _context.Addresses.Remove(address);
    }
}

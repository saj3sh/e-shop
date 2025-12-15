using Microsoft.EntityFrameworkCore;
using EShop.Domain.Customers;

namespace EShop.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken ct = default)
    {
        return await _context.Customers.FindAsync([id], ct);
    }

    public async Task<Address?> GetAddressAsync(Guid addressId, CancellationToken ct = default)
    {
        return await _context.Addresses.FindAsync([addressId], ct);
    }

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
    {
        await _context.Customers.AddAsync(customer, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddAddressAsync(Address address, CancellationToken ct = default)
    {
        await _context.Addresses.AddAsync(address, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync(ct);
    }
}

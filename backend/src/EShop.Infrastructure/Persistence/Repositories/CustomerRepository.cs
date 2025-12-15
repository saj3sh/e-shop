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

    public async Task<List<Address>> GetCustomerAddressesAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Addresses
            .Where(a => a.CustomerId == customerId)
            .ToListAsync(ct);
    }

    public void Add(Customer customer)
    {
        _context.Customers.Add(customer);
    }

    public void AddAddress(Address address)
    {
        _context.Addresses.Add(address);
    }

    public void Update(Customer customer)
    {
        _context.Customers.Update(customer);
    }
}

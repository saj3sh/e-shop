using Microsoft.EntityFrameworkCore;
using EShop.Domain.Orders;
using EShop.Domain.Customers;

namespace EShop.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default)
    {
        return await _context.Orders.FindAsync([id], ct);
    }

    public async Task<List<Order>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken ct = default)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.PurchaseDate)
            .ToListAsync(ct);
    }

    public async Task<List<Order>> GetIncompleteOrdersAsync(CancellationToken ct = default)
    {
        return await _context.Orders
            .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .OrderBy(o => o.PurchaseDate)
            .ToListAsync(ct);
    }

    public void Add(Order order)
    {
        _context.Orders.Add(order);
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }
}

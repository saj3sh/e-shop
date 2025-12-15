namespace EShop.Domain.Orders;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default);
    Task<List<Order>> GetByCustomerIdAsync(Customers.CustomerId customerId, CancellationToken ct = default);
    Task<List<Order>> GetIncompleteOrdersAsync(CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}

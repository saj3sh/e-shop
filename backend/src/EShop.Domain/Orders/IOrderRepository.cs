namespace EShop.Domain.Orders;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default);
    Task<List<Order>> GetByCustomerIdAsync(Customers.CustomerId customerId, CancellationToken ct = default);
    Task<List<Order>> GetIncompleteOrdersAsync(CancellationToken ct = default);
    void Add(Order order);
    void Update(Order order);
}

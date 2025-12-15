namespace EShop.Domain.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken ct = default);
    Task<(List<Product> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Product> products, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task<Product?> FindByNameAndPriceAsync(string name, decimal price, CancellationToken ct = default);
}

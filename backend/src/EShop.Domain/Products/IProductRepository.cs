namespace EShop.Domain.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken ct = default);
    Task<(List<Product> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize, CancellationToken ct = default);
    void Add(Product product);
    void AddRange(IEnumerable<Product> products);
    void Update(Product product);
    Task<Product?> FindByNameAndPriceAsync(string name, decimal price, CancellationToken ct = default);
}

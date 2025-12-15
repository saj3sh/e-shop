using Microsoft.EntityFrameworkCore;
using EShop.Domain.Products;
using EShop.Application.Common;

namespace EShop.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    private readonly ICacheInvalidator _cacheInvalidator;

    public ProductRepository(AppDbContext context, ICacheInvalidator cacheInvalidator)
    {
        _context = context;
        _cacheInvalidator = cacheInvalidator;
    }

    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken ct = default)
    {
        return await _context.Products.FindAsync([id], ct);
    }

    public async Task<List<Product>> GetByIdsAsync(List<ProductId> ids, CancellationToken ct = default)
    {
        return await _context.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);
    }

    public async Task<(List<Product> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public void Add(Product product)
    {
        _context.Products.Add(product);
    }

    public void AddRange(IEnumerable<Product> products)
    {
        _context.Products.AddRange(products);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);

        // Note: Cache invalidation will happen after UnitOfWork.SaveChanges
        _cacheInvalidator.InvalidateProductAsync(product.Id.Value, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task<Product?> FindByNameAndPriceAsync(string name, decimal price, CancellationToken ct = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Name == name && p.Price.Amount == price, ct);
    }
}

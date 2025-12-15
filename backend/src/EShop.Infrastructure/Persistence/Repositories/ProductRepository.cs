using Microsoft.EntityFrameworkCore;
using EShop.Domain.Products;

namespace EShop.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken ct = default)
    {
        return await _context.Products.FindAsync(new object[] { id }, ct);
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

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await _context.Products.AddAsync(product, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<Product> products, CancellationToken ct = default)
    {
        await _context.Products.AddRangeAsync(products, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Product?> FindByNameAndPriceAsync(string name, decimal price, CancellationToken ct = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Name == name && p.Price.Amount == price, ct);
    }
}

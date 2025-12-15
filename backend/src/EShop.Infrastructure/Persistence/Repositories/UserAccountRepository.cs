using Microsoft.EntityFrameworkCore;
using EShop.Domain.Auth;
using EShop.Domain.Customers;

namespace EShop.Infrastructure.Persistence.Repositories;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly AppDbContext _context;

    public UserAccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserAccount?> GetByIdAsync(UserAccountId id, CancellationToken ct = default)
    {
        return await _context.UserAccounts.FindAsync([id], ct);
    }

    public async Task<UserAccount?> GetByEmailAsync(Email email, CancellationToken ct = default)
    {
        return await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<UserAccount?> GetByCustomerIdAsync(CustomerId customerId, CancellationToken ct = default)
    {
        return await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.CustomerId == customerId, ct);
    }

    public async Task<UserAccount?> GetByRefreshTokenAsync(string token, CancellationToken ct = default)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, ct);

        if (refreshToken == null)
            return null;

        return await _context.UserAccounts
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == refreshToken.UserAccountId, ct);
    }

    public async Task AddAsync(UserAccount account, CancellationToken ct = default)
    {
        await _context.UserAccounts.AddAsync(account, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(UserAccount account, CancellationToken ct = default)
    {
        // EF Core auto-detects changes. so just save
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, ct);
        await _context.SaveChangesAsync(ct);
    }
}

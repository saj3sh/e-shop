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
        return await _context.UserAccounts.FindAsync(new object[] { id }, ct);
    }

    public async Task<UserAccount?> GetByEmailAsync(Email email, CancellationToken ct = default)
    {
        // Use AsNoTracking to avoid entity tracking issues
        return await _context.UserAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<UserAccount?> GetByRefreshTokenAsync(string token, CancellationToken ct = default)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, ct);

        if (refreshToken == null)
            return null;

        // Load user with refresh tokens for token refresh operation
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
        // Attach the untracked entity and mark only specific properties as modified
        var entry = _context.Entry(account);
        if (entry.State == EntityState.Detached)
        {
            _context.UserAccounts.Attach(account);
            entry.Property(u => u.LastLoginAt).IsModified = true;
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, ct);
        await _context.SaveChangesAsync(ct);
    }
}

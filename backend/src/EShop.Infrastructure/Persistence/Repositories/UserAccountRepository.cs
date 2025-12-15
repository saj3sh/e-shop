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

    public void Add(UserAccount account)
    {
        _context.UserAccounts.Add(account);
    }

    public void Update(UserAccount account)
    {
        _context.UserAccounts.Update(account);
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
    }
}

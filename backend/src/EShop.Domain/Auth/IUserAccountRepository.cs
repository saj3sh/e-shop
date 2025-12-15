namespace EShop.Domain.Auth;

public interface IUserAccountRepository
{
    Task<UserAccount?> GetByIdAsync(UserAccountId id, CancellationToken ct = default);
    Task<UserAccount?> GetByEmailAsync(Customers.Email email, CancellationToken ct = default);
    Task<UserAccount?> GetByCustomerIdAsync(Customers.CustomerId customerId, CancellationToken ct = default);
    Task<UserAccount?> GetByRefreshTokenAsync(string token, CancellationToken ct = default);
    void Add(UserAccount account);
    void Update(UserAccount account);
    void AddRefreshToken(RefreshToken refreshToken);
}

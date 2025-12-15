namespace EShop.Domain.Auth;

/// <summary>
/// user account aggregate for authentication
/// </summary>
public class UserAccount : Common.AggregateRoot
{
    public UserAccountId Id { get; private set; } = null!;
    public Customers.Email Email { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public Customers.CustomerId? CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private UserAccount() { }

    public UserAccount(UserAccountId id, Customers.Email email, UserRole role, Customers.CustomerId? customerId = null)
    {
        Id = id;
        Email = email;
        Role = role;
        CustomerId = customerId;
        CreatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public RefreshToken AddRefreshToken(string token, DateTime expiresAt)
    {
        var refreshToken = new RefreshToken(Guid.NewGuid(), Id, token, expiresAt, DateTime.UtcNow);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public void RevokeRefreshToken(string token)
    {
        var existing = _refreshTokens.FirstOrDefault(t => t.Token == token);
        if (existing != null)
        {
            existing.Revoke();
        }
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens.Where(t => !t.IsRevoked))
        {
            token.Revoke();
        }
    }
}

public record UserAccountId(Guid Value) : Common.EntityId<Guid>(Value)
{
    public static UserAccountId New() => new(Guid.NewGuid());
}

public enum UserRole
{
    User,
    Admin
}

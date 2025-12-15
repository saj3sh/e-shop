namespace EShop.Domain.Auth;

/// <summary>
/// refresh token entity for token rotation
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; }
    public UserAccountId UserAccountId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public RefreshToken(Guid id, UserAccountId userAccountId, string token, DateTime expiresAt, DateTime createdAt)
    {
        Id = id;
        UserAccountId = userAccountId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        IsRevoked = false;
    }

    private RefreshToken() { }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsValid() => !IsRevoked && ExpiresAt > DateTime.UtcNow;
}

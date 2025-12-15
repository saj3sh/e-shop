using EShop.Domain.Auth;

namespace EShop.Application.Common;

/// <summary>
/// token service abstraction for application layer
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(UserAccount user);
    string GenerateRefreshToken();
}

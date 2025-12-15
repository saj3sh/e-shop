using EShop.Application.Common;

namespace EShop.Application.Auth;

public record RefreshTokenCommand(string RefreshToken) : ICommand<Result<RefreshTokenResult>>;

public record RefreshTokenResult(string AccessToken, string RefreshToken);

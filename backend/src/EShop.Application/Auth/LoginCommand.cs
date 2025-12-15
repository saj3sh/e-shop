using EShop.Domain.Auth;
using EShop.Application.Common;

namespace EShop.Application.Auth;

public record LoginCommand(string Email) : ICommand<Result<LoginResult>>;

public record LoginResult(string AccessToken, string RefreshToken, UserRole Role, Guid UserId);

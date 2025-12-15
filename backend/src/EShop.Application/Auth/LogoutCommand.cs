using EShop.Application.Common;

namespace EShop.Application.Auth;

public record LogoutCommand(string RefreshToken) : ICommand<Result>;

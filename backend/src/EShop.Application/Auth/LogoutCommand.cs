using EShop.Domain.Auth;
using EShop.Application.Common;

namespace EShop.Application.Auth;

public record LogoutCommand(string RefreshToken) : ICommand<Result>;

public class LogoutCommandHandler : ICommandHandler<LogoutCommand, Result>
{
    private readonly IUserAccountRepository _userAccountRepo;

    public LogoutCommandHandler(IUserAccountRepository userAccountRepo)
    {
        _userAccountRepo = userAccountRepo;
    }

    public async Task<Result> HandleAsync(LogoutCommand command, CancellationToken ct = default)
    {
        var user = await _userAccountRepo.GetByRefreshTokenAsync(command.RefreshToken, ct);

        if (user == null)
            return Result.Success(); // already logged out

        user.RevokeRefreshToken(command.RefreshToken);
        await _userAccountRepo.UpdateAsync(user, ct);

        return Result.Success();
    }
}

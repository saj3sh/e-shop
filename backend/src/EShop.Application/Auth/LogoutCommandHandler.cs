using EShop.Domain.Auth;
using EShop.Application.Common;

namespace EShop.Application.Auth;

public class LogoutCommandHandler : ICommandHandler<LogoutCommand, Result>
{
    private readonly IUserAccountRepository _userAccountRepo;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUserAccountRepository userAccountRepo, IUnitOfWork unitOfWork)
    {
        _userAccountRepo = userAccountRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(LogoutCommand command, CancellationToken ct = default)
    {
        var user = await _userAccountRepo.GetByRefreshTokenAsync(command.RefreshToken, ct);

        if (user == null)
            return Result.Success(); // already logged out

        user.RevokeRefreshToken(command.RefreshToken);
        _userAccountRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

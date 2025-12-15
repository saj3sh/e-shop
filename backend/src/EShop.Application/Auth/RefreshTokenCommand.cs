using EShop.Domain.Auth;
using EShop.Application.Common;

namespace EShop.Application.Auth;

public record RefreshTokenCommand(string RefreshToken) : ICommand<Result<RefreshTokenResult>>;

public record RefreshTokenResult(string AccessToken, string RefreshToken);

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResult>>
{
    private readonly IUserAccountRepository _userAccountRepo;
    private readonly ITokenService _jwtService;

    public RefreshTokenCommandHandler(
        IUserAccountRepository userAccountRepo,
        ITokenService jwtService)
    {
        _userAccountRepo = userAccountRepo;
        _jwtService = jwtService;
    }

    public async Task<Result<RefreshTokenResult>> HandleAsync(RefreshTokenCommand command, CancellationToken ct = default)
    {
        var user = await _userAccountRepo.GetByRefreshTokenAsync(command.RefreshToken, ct);

        if (user == null)
            return Result<RefreshTokenResult>.Failure("Invalid refresh token");

        var existingToken = user.RefreshTokens.FirstOrDefault(t => t.Token == command.RefreshToken);

        if (existingToken == null || !existingToken.IsValid())
            return Result<RefreshTokenResult>.Failure("Refresh token expired or revoked");

        // revoke old token
        user.RevokeRefreshToken(command.RefreshToken);

        // generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken(
            Guid.NewGuid(),
            user.Id,
            newRefreshToken,
            DateTime.UtcNow.AddDays(7),
            DateTime.UtcNow
        );
        await _userAccountRepo.AddRefreshTokenAsync(refreshTokenEntity, ct);
        await _userAccountRepo.UpdateAsync(user, ct);

        return Result<RefreshTokenResult>.Success(new RefreshTokenResult(
            newAccessToken,
            newRefreshToken
        ));
    }
}

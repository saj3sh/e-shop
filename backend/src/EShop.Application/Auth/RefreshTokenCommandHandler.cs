using EShop.Domain.Auth;
using EShop.Application.Common;

namespace EShop.Application.Auth;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResult>>
{
    private readonly IUserAccountRepository _userAccountRepo;
    private readonly ITokenService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IUserAccountRepository userAccountRepo,
        ITokenService jwtService,
        IUnitOfWork unitOfWork)
    {
        _userAccountRepo = userAccountRepo;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RefreshTokenResult>> HandleAsync(RefreshTokenCommand command, CancellationToken ct = default)
    {
        try
        {
            var user = await _userAccountRepo.GetByRefreshTokenAsync(command.RefreshToken, ct);

            if (user == null)
                return Result<RefreshTokenResult>.Failure("Invalid refresh token");

            var existingToken = user.RefreshTokens.FirstOrDefault(t => t.Token == command.RefreshToken);

            if (existingToken == null || !existingToken.IsValid())
                return Result<RefreshTokenResult>.Failure("Refresh token expired or revoked");

            await _unitOfWork.BeginTransactionAsync(ct);

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
            _userAccountRepo.AddRefreshToken(refreshTokenEntity);
            _userAccountRepo.Update(user);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            return Result<RefreshTokenResult>.Success(new RefreshTokenResult(
                newAccessToken,
                newRefreshToken
            ));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            return Result<RefreshTokenResult>.Failure($"token refresh failed: {ex.Message}");
        }
    }
}

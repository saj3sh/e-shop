using EShop.Domain.Auth;
using EShop.Domain.Customers;
using EShop.Application.Common;

namespace EShop.Application.Auth;

public class LoginCommandHandler : ICommandHandler<LoginCommand, Result<LoginResult>>
{
    private readonly IUserAccountRepository _userAccountRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly ITokenService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserAccountRepository userAccountRepo,
        ICustomerRepository customerRepo,
        ITokenService jwtService,
        IUnitOfWork unitOfWork)
    {
        _userAccountRepo = userAccountRepo;
        _customerRepo = customerRepo;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResult>> HandleAsync(LoginCommand command, CancellationToken ct = default)
    {
        try
        {
            var email = Email.Create(command.Email);

            var user = await _userAccountRepo.GetByEmailAsync(email, ct);

            if (user == null)
            {
                return Result<LoginResult>.Failure("user not found", "USER_NOT_FOUND");
            }

            await _unitOfWork.BeginTransactionAsync(ct);

            user.RecordLogin(command.IpAddress);
            _userAccountRepo.Update(user);

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // store refresh token to DB. better to store in redis in prod.
            var refreshTokenEntity = new RefreshToken(
                Guid.NewGuid(),
                user.Id,
                refreshToken,
                DateTime.UtcNow.AddDays(7),
                DateTime.UtcNow
            );
            _userAccountRepo.AddRefreshToken(refreshTokenEntity);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            return Result<LoginResult>.Success(new LoginResult(
                accessToken,
                refreshToken,
                user.Role,
                user.Id.Value
            ));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            return Result<LoginResult>.Failure($"login failed: {ex.Message}");
        }
    }
}

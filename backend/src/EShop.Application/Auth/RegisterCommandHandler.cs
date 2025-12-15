using EShop.Domain.Auth;
using EShop.Domain.Customers;
using EShop.Application.Common;

namespace EShop.Application.Auth;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, Result<RegisterResult>>
{
    private readonly IUserAccountRepository _userAccountRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserAccountRepository userAccountRepo,
        ICustomerRepository customerRepo,
        IUnitOfWork unitOfWork)
    {
        _userAccountRepo = userAccountRepo;
        _customerRepo = customerRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegisterResult>> HandleAsync(RegisterCommand command, CancellationToken ct = default)
    {
        try
        {
            var email = Email.Create(command.Email);
            var phone = Phone.Create(command.Phone);

            // check if user already exists
            var existingUser = await _userAccountRepo.GetByEmailAsync(email, ct);
            if (existingUser != null)
            {
                return Result<RegisterResult>.Failure("User with this email already exists");
            }

            // Begin transaction for multi-aggregate operation
            await _unitOfWork.BeginTransactionAsync(ct);

            // check if customer exists
            // create customer
            var customer = new Customer(
                CustomerId.New(),
                command.FirstName,
                command.LastName,
                phone
            );
            _customerRepo.Add(customer);

            // create shipping address
            var shippingAddress = new Address(
                Guid.NewGuid(),
                command.ShippingAddress,
                command.ShippingCity,
                command.ShippingCountryCode,
                AddressType.Shipping,
                customer.Id.Value
            );
            _customerRepo.AddAddress(shippingAddress);

            // create billing address (if different from shipping)
            Address billingAddress;
            if (command.BillingAddress == command.ShippingAddress &&
                command.BillingCity == command.ShippingCity &&
                command.BillingCountryCode == command.ShippingCountryCode)
            {
                // same address, update shipping to Both type
                shippingAddress.UpdateType(AddressType.Both);
                billingAddress = shippingAddress;
            }
            else
            {
                billingAddress = new Address(
                    Guid.NewGuid(),
                    command.BillingAddress,
                    command.BillingCity,
                    command.BillingCountryCode,
                    AddressType.Billing,
                    customer.Id.Value
                );
                _customerRepo.AddAddress(billingAddress);
            }

            // set default addresses
            customer.SetDefaultShippingAddress(shippingAddress.Id);
            customer.SetDefaultBillingAddress(billingAddress.Id);

            // create user account
            var user = new UserAccount(
                UserAccountId.New(),
                email,
                UserRole.User,
                customer.Id
            );
            _userAccountRepo.Add(user);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            return Result<RegisterResult>.Success(new RegisterResult(
                user.Id.Value,
                user.Email.Value
            ));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            return Result<RegisterResult>.Failure($"registration failed: {ex.Message}");
        }
    }
}

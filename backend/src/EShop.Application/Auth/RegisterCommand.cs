using EShop.Domain.Auth;
using EShop.Domain.Customers;
using EShop.Application.Common;

namespace EShop.Application.Auth;

public record RegisterCommand(
    string Email,
    string FirstName,
    string LastName,
    string Phone,
    string ShippingAddress,
    string ShippingCity,
    string ShippingCountry,
    string BillingAddress,
    string BillingCity,
    string BillingCountry
) : ICommand<Result<RegisterResult>>;

public record RegisterResult(Guid UserId, string Email);

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, Result<RegisterResult>>
{
    private readonly IUserAccountRepository _userAccountRepo;
    private readonly ICustomerRepository _customerRepo;

    public RegisterCommandHandler(
        IUserAccountRepository userAccountRepo,
        ICustomerRepository customerRepo)
    {
        _userAccountRepo = userAccountRepo;
        _customerRepo = customerRepo;
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
                return Result<RegisterResult>.Failure("user with this email already exists");
            }

            // check if customer exists
            // create customer
            var customer = new Customer(
                CustomerId.New(),
                command.FirstName,
                command.LastName,
                email,
                phone
            );
            await _customerRepo.AddAsync(customer, ct);

            // create shipping address
            var shippingAddress = new Address(
                Guid.NewGuid(),
                command.ShippingAddress,
                command.ShippingCity,
                command.ShippingCountry,
                AddressType.Shipping,
                customer.Id.Value
            );
            await _customerRepo.AddAddressAsync(shippingAddress, ct);

            // create billing address (if different from shipping)
            Address billingAddress;
            if (command.BillingAddress == command.ShippingAddress &&
                command.BillingCity == command.ShippingCity &&
                command.BillingCountry == command.ShippingCountry)
            {
                // same address, update shipping to Both type
                billingAddress = new Address(
                    shippingAddress.Id,
                    command.ShippingAddress,
                    command.ShippingCity,
                    command.ShippingCountry,
                    AddressType.Both,
                    customer.Id.Value
                );
            }
            else
            {
                billingAddress = new Address(
                    Guid.NewGuid(),
                    command.BillingAddress,
                    command.BillingCity,
                    command.BillingCountry,
                    AddressType.Billing,
                    customer.Id.Value
                );
                await _customerRepo.AddAddressAsync(billingAddress, ct);
            }

            // set default addresses
            customer.SetDefaultShippingAddress(shippingAddress.Id);
            customer.SetDefaultBillingAddress(billingAddress.Id);
            await _customerRepo.UpdateAsync(customer, ct);

            // create user account
            var user = new UserAccount(
                UserAccountId.New(),
                email,
                UserRole.User,
                customer.Id
            );
            await _userAccountRepo.AddAsync(user, ct);

            return Result<RegisterResult>.Success(new RegisterResult(
                user.Id.Value,
                user.Email.Value
            ));
            await _userAccountRepo.AddAsync(user, ct);

            return Result<RegisterResult>.Success(new RegisterResult(
                user.Id.Value,
                user.Email.Value
            ));
        }
        catch (Exception ex)
        {
            return Result<RegisterResult>.Failure($"registration failed: {ex.Message}");
        }
    }
}

using EShop.Domain.Customers;
using EShop.Application.Common;

namespace EShop.Application.Customers;

public record UpdateCustomerCommand(Guid Id, string FirstName, string LastName, string Phone) : ICommand<Result>;

public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand, Result>
{
    private readonly ICustomerRepository _customerRepo;

    public UpdateCustomerCommandHandler(ICustomerRepository customerRepo)
    {
        _customerRepo = customerRepo;
    }

    public async Task<Result> HandleAsync(UpdateCustomerCommand command, CancellationToken ct = default)
    {
        try
        {
            var customer = await _customerRepo.GetByIdAsync(new CustomerId(command.Id), ct);

            if (customer == null)
                return Result.Failure("customer not found");

            var phone = Phone.Create(command.Phone);
            customer.UpdateProfile(command.FirstName, command.LastName, phone);

            await _customerRepo.UpdateAsync(customer, ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"update failed: {ex.Message}");
        }
    }
}

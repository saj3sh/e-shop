using EShop.Domain.Customers;
using EShop.Application.Common;

namespace EShop.Application.Customers;

public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand, Result>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(ICustomerRepository customerRepo, IUnitOfWork unitOfWork)
    {
        _customerRepo = customerRepo;
        _unitOfWork = unitOfWork;
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

            _customerRepo.Update(customer);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"update failed: {ex.Message}");
        }
    }
}

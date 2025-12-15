using EShop.Domain.Customers;
using EShop.Domain.Addresses;
using EShop.Application.Common;

namespace EShop.Application.Addresses;

public class DeleteAddressCommandHandler : ICommandHandler<DeleteAddressCommand, Result>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IAddressRepository _addressRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAddressCommandHandler(ICustomerRepository customerRepo, IAddressRepository addressRepo, IUnitOfWork unitOfWork)
    {
        _customerRepo = customerRepo;
        _addressRepo = addressRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(DeleteAddressCommand command, CancellationToken ct = default)
    {
        var customer = await _customerRepo.GetByIdAsync(new CustomerId(command.CustomerId), ct);
        if (customer == null)
            return Result.Failure("Customer not found");

        var address = await _addressRepo.GetByIdAsync(command.AddressId, ct);
        if (address == null || address.CustomerId != command.CustomerId)
            return Result.Failure("Address not found");

        if (customer.DefaultShippingAddressId == command.AddressId)
            return Result.Failure("Cannot delete default shipping address");

        if (customer.DefaultBillingAddressId == command.AddressId)
            return Result.Failure("Cannot delete default billing address");

        _addressRepo.Delete(address);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

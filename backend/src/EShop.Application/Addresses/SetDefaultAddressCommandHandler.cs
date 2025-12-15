using EShop.Domain.Customers;
using EShop.Domain.Addresses;
using EShop.Application.Common;

namespace EShop.Application.Addresses;

public class SetDefaultAddressCommandHandler : ICommandHandler<SetDefaultAddressCommand, Result>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IAddressRepository _addressRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SetDefaultAddressCommandHandler(ICustomerRepository customerRepo, IAddressRepository addressRepo, IUnitOfWork unitOfWork)
    {
        _customerRepo = customerRepo;
        _addressRepo = addressRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(SetDefaultAddressCommand command, CancellationToken ct = default)
    {
        // Verify customer exists
        var customer = await _customerRepo.GetByIdAsync(new CustomerId(command.CustomerId), ct);
        if (customer == null)
            return Result.Failure("Customer not found");

        // Verify address exists and belongs to customer
        var address = await _addressRepo.GetByIdAsync(command.AddressId, ct);
        if (address == null || address.CustomerId != command.CustomerId)
            return Result.Failure("Address not found");

        // Set default address based on type
        if (command.AddressType == "Shipping")
            customer.SetDefaultShippingAddress(command.AddressId);
        else if (command.AddressType == "Billing")
            customer.SetDefaultBillingAddress(command.AddressId);
        else
            return Result.Failure("Invalid address type");

        _customerRepo.Update(customer);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

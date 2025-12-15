using EShop.Domain.Customers;
using EShop.Domain.Addresses;
using EShop.Application.Common;

namespace EShop.Application.Addresses;

public class CreateAddressCommandHandler : ICommandHandler<CreateAddressCommand, Result<Guid>>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IAddressRepository _addressRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAddressCommandHandler(ICustomerRepository customerRepo, IAddressRepository addressRepo, IUnitOfWork unitOfWork)
    {
        _customerRepo = customerRepo;
        _addressRepo = addressRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> HandleAsync(CreateAddressCommand command, CancellationToken ct = default)
    {
        // Verify customer exists
        var customer = await _customerRepo.GetByIdAsync(new CustomerId(command.CustomerId), ct);
        if (customer == null)
            return Result<Guid>.Failure("Customer not found");

        // Parse and validate address type
        if (!Enum.TryParse<AddressType>(command.Type, out var addressType))
            return Result<Guid>.Failure("Invalid address type");

        // Create address
        var address = new Address(
            Guid.NewGuid(),
            command.Line1,
            command.City,
            command.Country,
            addressType,
            command.CustomerId
        );

        _addressRepo.Add(address);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(address.Id);
    }
}

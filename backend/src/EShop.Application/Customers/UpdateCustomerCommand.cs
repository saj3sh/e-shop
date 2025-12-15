using EShop.Application.Common;

namespace EShop.Application.Customers;

public record UpdateCustomerCommand(Guid Id, string FirstName, string LastName, string Phone) : ICommand<Result>;

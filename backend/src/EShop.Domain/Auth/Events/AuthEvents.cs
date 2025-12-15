namespace EShop.Domain.Auth;

public record UserLoggedIn(Guid UserId, string Email, string? IpAddress, DateTime OccurredAt) : Common.IDomainEvent;

public record UserLoggedOut(Guid UserId, string Email, DateTime OccurredAt) : Common.IDomainEvent;

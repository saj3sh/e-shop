using EShop.Application.Common;
using EShop.Domain.Auth;
using EShop.Domain.Common;

namespace EShop.Application.Auth.EventHandlers;

/// <summary>
/// Domain event handler for UserLoggedIn event
/// </summary>
public class UserLoggedInEventHandler : IDomainEventHandler<UserLoggedIn>
{
    private readonly IActivityLogRepository _activityLogRepo;

    public UserLoggedInEventHandler(IActivityLogRepository activityLogRepo)
    {
        _activityLogRepo = activityLogRepo;
    }

    public async Task HandleAsync(UserLoggedIn domainEvent, CancellationToken cancellationToken = default)
    {
        // Log user login activity
        var activityLog = ActivityLog.Create(
            entityType: "UserAccount",
            entityId: domainEvent.UserId.ToString(),
            action: "UserLoggedIn",
            userId: domainEvent.UserId.ToString(),
            userEmail: domainEvent.Email,
            details: $"User {domainEvent.Email} logged in",
            ipAddress: domainEvent.IpAddress
        );

        await _activityLogRepo.AddAsync(activityLog, cancellationToken);

        // TODO: Additional actions
        // - Send login notification email
        // - Check for suspicious login patterns
        // - Update user statistics
    }
}

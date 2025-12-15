using EShop.Application.Common;
using EShop.Domain.Auth;
using EShop.Domain.Common;

namespace EShop.Application.Auth.EventHandlers;

/// <summary>
/// Domain event handler for UserLoggedOut event
/// </summary>
public class UserLoggedOutEventHandler : IDomainEventHandler<UserLoggedOut>
{
    private readonly IActivityLogRepository _activityLogRepo;

    public UserLoggedOutEventHandler(IActivityLogRepository activityLogRepo)
    {
        _activityLogRepo = activityLogRepo;
    }

    public async Task HandleAsync(UserLoggedOut domainEvent, CancellationToken cancellationToken = default)
    {
        // Log user logout activity
        var activityLog = ActivityLog.Create(
            entityType: "UserAccount",
            entityId: domainEvent.UserId.ToString(),
            action: "UserLoggedOut",
            userId: domainEvent.UserId.ToString(),
            userEmail: domainEvent.Email,
            details: $"User {domainEvent.Email} logged out"
        );

        await _activityLogRepo.AddAsync(activityLog, cancellationToken);

        // TODO: Additional actions
        // - Clean up session data
        // - Update last activity timestamp
    }
}

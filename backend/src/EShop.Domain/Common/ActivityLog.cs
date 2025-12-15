namespace EShop.Domain.Common;

/// <summary>
/// Activity log entity for tracking user and system actions
/// </summary>
public class ActivityLog
{
    public Guid Id { get; private set; }
    public string EntityType { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;
    public string Action { get; private set; } = null!;
    public string? UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? Details { get; private set; }
    public string? IpAddress { get; private set; }

    private ActivityLog() { }

    public static ActivityLog Create(
        string entityType,
        string entityId,
        string action,
        string? userId = null,
        string? userEmail = null,
        string? details = null,
        string? ipAddress = null)
    {
        return new ActivityLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            UserEmail = userEmail,
            Timestamp = DateTime.UtcNow,
            Details = details,
            IpAddress = ipAddress
        };
    }
}

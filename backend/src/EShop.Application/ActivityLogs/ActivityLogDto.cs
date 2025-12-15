namespace EShop.Application.ActivityLogs;

public record ActivityLogDto(
    Guid Id,
    string EntityType,
    string EntityId,
    string Action,
    string? UserId,
    string? UserEmail,
    DateTime Timestamp,
    string? Details,
    string? IpAddress
);

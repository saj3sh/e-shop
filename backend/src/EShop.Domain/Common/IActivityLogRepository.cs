namespace EShop.Domain.Common;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog activityLog, CancellationToken ct = default);
    Task<List<ActivityLog>> FilterAsync(string EntityType, int limit = 100, CancellationToken ct = default);
}

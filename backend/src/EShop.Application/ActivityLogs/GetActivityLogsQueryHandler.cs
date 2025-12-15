using EShop.Application.Common;
using EShop.Domain.Common;

namespace EShop.Application.ActivityLogs;

public class GetActivityLogsQueryHandler : IQueryHandler<GetActivityLogsQuery, Result<List<ActivityLogDto>>>
{
    private readonly IActivityLogRepository _activityLogRepo;

    public GetActivityLogsQueryHandler(IActivityLogRepository activityLogRepo)
    {
        _activityLogRepo = activityLogRepo;
    }

    public async Task<Result<List<ActivityLogDto>>> HandleAsync(GetActivityLogsQuery query, CancellationToken ct = default)
    {
        var limit = query.Limit > 0 ? query.Limit : 100;

        List<ActivityLog> logs = await _activityLogRepo.FilterAsync(query.EntityType!, query.Limit, ct);

        var dtos = logs.Take(limit).Select(log => new ActivityLogDto(
            log.Id,
            log.EntityType,
            log.EntityId,
            log.Action,
            log.UserId,
            log.UserEmail,
            log.Timestamp,
            log.Details,
            log.IpAddress
        )).ToList();

        return Result<List<ActivityLogDto>>.Success(dtos);
    }
}

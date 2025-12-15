using EShop.Application.Common;

namespace EShop.Application.ActivityLogs;

public record GetActivityLogsQuery(
    string? EntityType = null,
    int Limit = 100
) : IQuery<Result<List<ActivityLogDto>>>;
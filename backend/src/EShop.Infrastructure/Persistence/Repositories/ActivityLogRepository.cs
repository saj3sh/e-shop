using Microsoft.EntityFrameworkCore;
using EShop.Domain.Common;

namespace EShop.Infrastructure.Persistence.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly AppDbContext _context;

    public ActivityLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ActivityLog activityLog, CancellationToken ct = default)
    {
        await _context.ActivityLogs.AddAsync(activityLog, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<ActivityLog>> FilterAsync(string? entityType, int limit = 100, CancellationToken ct = default)
    {
        var query = _context.ActivityLogs.AsQueryable();

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync(ct);
    }
}

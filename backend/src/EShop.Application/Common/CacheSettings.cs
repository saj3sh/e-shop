namespace EShop.Application.Common;

public class CacheSettings
{
    public int ProductCacheMinutes { get; set; } = 5;
    public int SearchCacheMinutes { get; set; } = 2;
    public int CustomerCacheMinutes { get; set; } = 10;
    public int OrderCacheMinutes { get; set; } = 5;
}

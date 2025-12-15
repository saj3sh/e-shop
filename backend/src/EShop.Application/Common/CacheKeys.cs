namespace EShop.Application.Common;

public static class CacheKeys
{
    public static string ProductById(Guid id) => $"products:id:{id}";
    public static string ProductSearch(string? searchTerm, int page, int pageSize)
        => $"products:search:{searchTerm ?? "all"}:{page}:{pageSize}";
}

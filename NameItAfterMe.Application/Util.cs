using Microsoft.EntityFrameworkCore;

namespace NameItAfterMe.Application;

internal static class Util
{
    public static async Task<PagedResult<T>> ToPaginatedResult<T>(
        this IQueryable<T> source, 
        int pageNumber, 
        int pageSize, 
        CancellationToken token = default)
    {
        var content = await source.ToListAsync(token);
        return new PagedResult<T>(pageNumber, pageSize, content);
    }
}

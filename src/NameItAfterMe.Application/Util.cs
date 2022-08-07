using Microsoft.EntityFrameworkCore;

namespace NameItAfterMe.Application;

internal static class Util
{
    /// <summary>
    ///     Evaluates the source queryable as a PagedResult object.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of item.
    /// </typeparam>
    /// <param name="source">
    ///     The source queryable.
    /// </param>
    /// <param name="pageNumber">
    ///     The page number.
    /// </param>
    /// <param name="pageSize">
    ///     The page size.
    /// </param>
    /// <param name="token">
    ///     The cancellation token, if any.
    /// </param>
    /// <returns>
    ///     The evaluated result.
    /// </returns>
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
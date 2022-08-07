using System.Collections;

namespace NameItAfterMe.Application;

public record PagedResult<T> : IEnumerable<T>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public IEnumerable<T> Results { get; init; }

    public PagedResult(
        int pageNumber,
        int pageSize,
        IEnumerable<T> results)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Results = results;
    }

    public IEnumerator<T> GetEnumerator() => Results.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Results).GetEnumerator();
}
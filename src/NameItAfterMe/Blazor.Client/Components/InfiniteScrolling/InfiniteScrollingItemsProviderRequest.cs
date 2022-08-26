namespace NameItAfterMe.Blazor.Client.Components.InfiniteScrolling;

public sealed class InfiniteScrollingItemsProviderRequest
{
    public int StartIndex { get; }
    public CancellationToken CancellationToken { get; }

    public InfiniteScrollingItemsProviderRequest(
        int startIndex,
        CancellationToken cancellationToken)
            => (StartIndex, CancellationToken) = (startIndex, cancellationToken);
}

public delegate Task<IEnumerable<T>> InfiniteScrollingItemsProviderRequestDelegate<T>(InfiniteScrollingItemsProviderRequest context);
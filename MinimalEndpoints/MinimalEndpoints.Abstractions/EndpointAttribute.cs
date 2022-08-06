namespace MinimalEndpoints;

/// <summary>
///     Add this attribute to a Mediatr IRequest<T> to instruct your ASP.Net backend
///     to host the request behind an automaticlly generated endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class EndpointAttribute : Attribute
{
    /// <summary>
    ///     The Http method. May be inferred from the IRequest<T> concrete class name.
    /// </summary>
    public HttpMethods? HttpMethod { get; init; }

    /// <summary>
    ///     The route. Defaults to the IRequest<T> concrete class name.
    /// </summary>
    public string? Route { get; init; }

    /// <summary>
    ///     The type of content this endpoint will return to the caller.
    /// </summary>
    public string? ContentType { get; init; }
}
namespace NameItAfterMe.Application.Infrastructure.Nasa.PictureOfTheDay
{
    /// <summary>
    ///     Handler to add append an API key to web requests.
    /// </summary>
    public class ApiKeyHandler : DelegatingHandler
    {
        private readonly string _apiKey;

        public ApiKeyHandler(string apiKey) => _apiKey = apiKey;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // if the uri is null here just pass along to refit and let it handle the unexpected scenario.
            if (request.RequestUri == null)
                return base.SendAsync(request, cancellationToken);

            request.RequestUri = new Uri(request.RequestUri.AbsoluteUri + $"?api_key={_apiKey}");

            return base.SendAsync(request, cancellationToken);
        }
    }
}
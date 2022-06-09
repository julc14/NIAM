namespace NameItAfterMe.Server;

public interface IConfigureRequestBuilder
{
    ConfigureRequestBuilder ParseRequestPropertiesFromRouteData();
    ConfigureRequestBuilder ParseRequestPropertiesFromQueryParameters();
    ConfigureRequestBuilder ParseRequestPropertiesFromBody();
}
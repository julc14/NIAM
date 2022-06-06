using Newtonsoft.Json;
using System.ComponentModel;

namespace NameItAfterMe.Server;

public class ConfigureRequestBuilder : IConfigureRequestBinding
{
    public Type RequestType { get; }
    public object Request { get; }
    public HttpContext Context { get; }

    public ConfigureRequestBuilder(Type requestType, HttpContext context)
    {
        RequestType = requestType;
        Context = context;
        Request = Activator.CreateInstance(RequestType)
            ?? throw new InvalidOperationException($"Failed to create instance of {requestType}");
    }

    public ConfigureRequestBuilder(object request, HttpContext context)
    {
        RequestType = request.GetType();
        Request = request;
        Context = context;
    }

    public ConfigureRequestBuilder ParseRequestPropertiesFromBody()
    {
        if (Context.Request.ContentLength > 0 && Context.Request.HasJsonContentType())
        {
            using var s = new MemoryStream();
            Context.Request.BodyReader.CopyToAsync(s, Context.RequestAborted);

            using var reader = new StreamReader(s);

            var serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            serializer.Populate(reader, Request);
        }

        return this;
    }

    public ConfigureRequestBuilder ParseRequestPropertiesFromQueryParameters()
    {
        var queryProperties =
            from requestProperty in RequestType.GetProperties()

            join query in Context.Request.Query
                on requestProperty.Name.ToLower() equals query.Key.ToLower()

            // check what happens when type cant find convertor.
            let typeConvertor = TypeDescriptor.GetConverter(requestProperty.PropertyType)
            let conversionAttempt = typeConvertor.ConvertFromString(query.Value.ToString())

            // check if type conversion issues being hidden here
            where conversionAttempt is not null

            select new
            {
                requestProperty,
                typedObjectValue = conversionAttempt
            };

        foreach (var item in queryProperties)
        {
            item.requestProperty.SetValue(Request, item.typedObjectValue);
        }

        return this;
    }

    public ConfigureRequestBuilder ParseRequestPropertiesFromRouteData()
    {
        var routeData = Context.GetRouteData().Values;

        var routeDataProperties =
            from requestProperty in RequestType.GetProperties()

            join route in routeData
                on requestProperty.Name.ToLower() equals route.Key.ToLower()

            // check what happens when type cant find convertor.
            let typeConvertor = TypeDescriptor.GetConverter(requestProperty.PropertyType)
            let conversionAttempt = typeConvertor.ConvertFromString(route.Value.ToString())

            // check if type conversion issues being hidden here
            where conversionAttempt is not null

            select new
            {
                requestProperty,
                typedObjectValue = conversionAttempt
            };

        foreach (var item in routeDataProperties)
        {
            item.requestProperty.SetValue(Request, item.typedObjectValue);
        }

        return this;
    }
}

public interface IConfigureRequestBinding
{
    ConfigureRequestBuilder ParseRequestPropertiesFromRouteData();
    ConfigureRequestBuilder ParseRequestPropertiesFromQueryParameters();
    ConfigureRequestBuilder ParseRequestPropertiesFromBody();
}
using Newtonsoft.Json;
using System.ComponentModel;

namespace NameItAfterMe.Server;

public class ConfigureRequestBuilder : IConfigureRequestBuilder
{
    private readonly HttpContext _context;

    public Type RequestType { get; }
    public object Request { get; }

    public ConfigureRequestBuilder(Type requestType, HttpContext context)
    {
        _context = context;
        RequestType = requestType;
        Request = Activator.CreateInstance(RequestType)
            ?? throw new InvalidOperationException($"Failed to create instance of {requestType}");
    }

    public ConfigureRequestBuilder(object request, HttpContext context)
    {
        _context = context;
        RequestType = request.GetType();
        Request = request;
    }

    public ConfigureRequestBuilder ParseRequestPropertiesFromBody()
    {
        if (_context.Request.ContentLength > 0 && _context.Request.HasJsonContentType())
        {
            using var s = new MemoryStream();
            _context.Request.BodyReader.CopyToAsync(s, _context.RequestAborted);

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

            join query in _context.Request.Query
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
        var routeData = _context.GetRouteData().Values;

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

namespace MinimalEndpoints;

public class ConfigureRequestBuilder 
{


    //public ConfigureRequestBuilder ParseRequestPropertiesFromBody()
    //{
    //    if (_context.Request.ContentLength > 0 && _context.Request.HasJsonContentType())
    //    {
    //        using var s = new MemoryStream();
    //        _context.Request.BodyReader.CopyToAsync(s, _context.RequestAborted);

    //        using var reader = new StreamReader(s);

    //        var serializer = new JsonSerializer()
    //        {
    //            NullValueHandling = NullValueHandling.Ignore,
    //            MissingMemberHandling = MissingMemberHandling.Ignore
    //        };

    //        serializer.Populate(reader, Request);
    //    }

    //    return this;
    //}
}
using System.Text.Json.Serialization;

namespace NameItAfterMe.Infrastructure;

public class ExoplanetQueryResponse
{
    [JsonPropertyName("sy_dist")]
    public decimal? Distance { get; set; }

    [JsonPropertyName("pl_name")]
    public string? Name { get; set; }
}
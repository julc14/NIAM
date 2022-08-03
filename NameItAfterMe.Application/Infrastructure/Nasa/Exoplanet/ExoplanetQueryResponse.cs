using System.Text.Json.Serialization;

namespace NameItAfterMe.Application.Infrastructure.Nasa.Exoplanet;

public class ExoplanetQueryResponse
{
    [JsonPropertyName("sy_dist")]
    public decimal? Distance { get; set; }

    [JsonPropertyName("pl_name")]
    public string? Name { get; set; }

    [JsonPropertyName("hostname")]
    public string? HostName { get; set; }

    [JsonPropertyName("disc_year")]
    public int? DiscoveryYear { get; set; }

    [JsonPropertyName("discoverymethod")]
    public string? DiscoveryMethod { get; set; }
}
using System.Text.Json.Serialization;

namespace GassiMeter;

public class HassEntity
{
    [JsonPropertyName("entity_id")]
    public string EntityId { get; set; } = string.Empty;
    
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("last_changed")]
    public DateTime LastChanged { get; set; }

    [JsonPropertyName("last_reported")]
    public DateTime LastReported { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTime LastUpdated { get; set; }

}
using System.Text.Json.Serialization;

namespace GassiMeter;

public class WledStateEntity
{
    [JsonPropertyName("on")]
    public bool IsOn { get; set; }
    [JsonPropertyName("bri")]
    public int Brightness { get; set; }
}

public class WledEntity
{
    [JsonPropertyName("state")]
    public WledStateEntity StateEntity { get; set; } 
}
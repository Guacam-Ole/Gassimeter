using System.Diagnostics.Contracts;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;

namespace GassiMeter;

public class WeatherEntity
{
    [JsonPropertyName("minutely")] public List<WeatherMinutelyEntity> Minutely { get; set; }
}

public class WeatherMinutelyEntity
{
    [JsonPropertyName("dt")] public long TimeStamp { get; set; }
    [JsonPropertyName("precipitation")] public double Rain { get; set; }
    public DateTime Time { get; set; } 
}

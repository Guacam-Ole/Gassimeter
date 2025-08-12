namespace GassiMeter;

public class Config
{
    public ConfigHass? Hass { get; set; } 
    public required ConfigWled Wled { get; set; } 

    public required ConfigWeather Weather { get; set; } 
    public ConfigTime? OperationTime { get; set; }
    
}

public abstract class ConfigTime
{
    public TimeSpan FromTime { get; set; }
    public TimeSpan ToTime { get; set; }
}


public abstract class ConfigWeather
{
    public double Longitude { get; set; } 
    public double Latitude { get; set; } 
    public TimeSpan Delay { get; set; } = new TimeSpan(0, 2, 0); 
}

public abstract class ConfigHass
{
    public string? Sensor { get; set; } 
    public string? RequiredState { get; set; } 

}

public abstract class ConfigWled
{
    public required string Url { get; set; }
    public int Count { get; set; } = 60;
    public int Start { get; set; } = 0;
    public int MinutesPerLed { get; set; } = 2;
    public int Brightness { get; set; } = 128;
}
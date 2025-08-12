namespace GassiMeter;

public class OpenWeather
{
    private readonly Rest _rest;
    private readonly Secrets _secrets;
    private readonly Config _config;

    public OpenWeather(Rest rest, Secrets secrets, Config config)
    {
        _rest = rest;
        _secrets = secrets;
        _config = config;
    }


    public async Task<WeatherEntity?> GetMinuteValues()
    {
        var latitude = _config.Weather.Latitude;
        var longitude =  _config.Weather.Longitude;
        
        var url=$"https://api.openweathermap.org/data/3.0/onecall?lat={latitude}&lon={longitude}&exclude=hourly,daily,alerts&appid={_secrets.WeatherApiKey}";
        var result= await _rest.Get<WeatherEntity>(url);
        
        if (result?.Minutely != null)
        {
            Console.WriteLine($"☁️ Successfully fetched '{result.Minutely.Count}' minute weather forecasts");
            result.Minutely.ForEach(q=>q.Time=q.TimeStamp.ToDateTime());
        }
        else
        {
            Console.WriteLine("🌩️ No weather minute data received from OpenWeather API");
        }
        
        return result;
    }
}
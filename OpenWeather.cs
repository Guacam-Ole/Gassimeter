using Microsoft.Extensions.Logging;

namespace GassiMeter;

public class OpenWeather
{
    private readonly Rest _rest;
    private readonly Secrets _secrets;
    private readonly Config _config;
    private readonly ILogger<OpenWeather> _logger;

    public OpenWeather(Rest rest, Secrets secrets, Config config, ILogger<OpenWeather> logger)
    {
        _rest = rest;
        _secrets = secrets;
        _config = config;
        _logger = logger;
    }


    public async Task<WeatherEntity?> GetMinuteValues()
    {
        var latitude = _config.Weather.Latitude;
        var longitude =  _config.Weather.Longitude;
        
        var url=$"https://api.openweathermap.org/data/3.0/onecall?lat={latitude}&lon={longitude}&exclude=hourly,daily,alerts&appid={_secrets.WeatherApiKey}";
        var result= await _rest.Get<WeatherEntity>(url);
        
        if (result?.Minutely != null)
        {
            _logger.LogInformation("☁️ Successfully fetched '{Count}' minute weather forecasts", result.Minutely.Count);
            result.Minutely.ForEach(q=>q.Time=q.TimeStamp.ToDateTime());
        }
        else
        {
            _logger.LogWarning("🌩️ No weather minute data received from OpenWeather API");
        }
        
        return result;
    }
}
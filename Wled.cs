using System.Text;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace GassiMeter;

public class Wled
{
    private readonly Config _config;
    private readonly Rest _rest;
    private readonly ILogger<Wled> _logger;

    public Wled(Config config, Rest rest, ILogger<Wled> logger)
    {
        _config = config;
        _rest = rest;
        _logger = logger;
    }

    public async Task TurnOn()
    {
        _logger.LogInformation("💡 Turning WLED on with brightness '{Brightness}'", _config.Wled.Brightness);
        var payload = "{\"on\":true,\"bri\":" + _config.Wled.Brightness + "} ";
        await PostPayload(payload);
    }

    public async Task TurnOff()
    {
        await ClearAllLeds();
        const string payload = "{\"off\":true } ";
        await PostPayload(payload);
        _logger.LogInformation("🔌 LEDs Turned Off");
    }


    public async Task ClearAllLeds()
    {
        _logger.LogInformation("🧩 Clearing '{Count}' LEDs", _config.Wled.Count);
        var payload = "{\"seg\":{\"i\":[";
        for (var i = 1; i <= _config.Wled.Count; i++)
        {
            payload += "\"000000\",";
        }

        payload = payload[..^1];
        payload += "]}}\"";
        await PostPayload(payload);
    }

    private async Task PostPayload(string payload)
    {
        // TODO: EST
        // TODO: Return
        using var client = new HttpClient();
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(_config.Wled.Url, content);
        response.EnsureSuccessStatusCode();
        Thread.Sleep(TimeSpan.FromSeconds(2));
    }

    public async Task<T?> GetStatus<T>()
    {
        return await _rest.Get<T?>(_config.Wled.Url);
    }

    public async Task LightLedAtMinute(int minute)
    {
        await ClearAllLeds();
        var led = GetLedFromMinute(minute);
        var payload = $"{{\"seg\":{{\"i\":[{led},\"FF0000\"]}}";
        await PostPayload(payload);
    }

    private int GetLedFromMinute(int minute)
    {
        var ledOffset = minute / _config.Wled.MinutesPerLed;
        var ledIndex = _config.Wled.Start + ledOffset;

        return ledIndex;
    }

    private static Color HexToColor(string hex)
    {
        var r = Convert.ToByte(hex[..2], 16);
        var g = Convert.ToByte(hex[2..4], 16);
        var b = Convert.ToByte(hex[4..6], 16);

        return Color.FromArgb(r, g, b);
    }

    private static string CalcGradientColor(Color minColor, double minValue, Color maxColor, double maxValue,
        double value)
    {
        var t = (value - minValue) / (maxValue - minValue);
        var red = (int)(minColor.R + t * (maxColor.R - minColor.R));
        var green = (int)(minColor.G + t * (maxColor.G - minColor.G));
        var blue = (int)(minColor.B + t * (maxColor.B - minColor.B));
        return red.ToString("X2") + green.ToString("X2") + blue.ToString("X2");
    }

    private string GetRgbColorByValue(double value)
    {
        switch (value)
        {
            case < 0:
                // Missing history value
                return "000000";
            case 0:
                return _config.Wled.Colors.First(q => q.RainAmount == 0).ColorCode;
        }

        var orderedColors = _config.Wled.Colors.OrderBy(q => q.RainAmount).ToList();
        for (var colorIndex = 1; colorIndex < _config.Wled.Colors.Count; colorIndex++)
        {
            var upperColor = orderedColors.ElementAt(colorIndex);
            var lowerColor = orderedColors.ElementAt(colorIndex - 1);
            if (value <= upperColor.RainAmount)
            {
                return CalcGradientColor(HexToColor(lowerColor.ColorCode),
                    lowerColor.RainAmount, HexToColor(upperColor.ColorCode),
                    upperColor.RainAmount, value);
            }
        }

        return _config.Wled.Colors.OrderBy(q => q.RainAmount).Last().ColorCode;
    }


    public async Task SetLedsByValueJson(Dictionary<int, double> minuteValues)
    {
        _logger.LogDebug("Sending values to LED: '{vals}'",string.Join(',', minuteValues));
        Dictionary<int, double> aggregatedValues = new();
        for (var i = 0; i <= _config.Wled.Count; i++)
        {
            aggregatedValues[i] = 0;
        }

        foreach (var minuteValue in minuteValues)
        {
            var led = GetLedFromMinute(minuteValue.Key);
            if (led < 0 || led > _config.Wled.Count) continue; // should never happen
            aggregatedValues[led] += minuteValue.Value;
        }

        var payload = "{\"seg\":{\"i\":[";
        for (var i = 0; i < aggregatedValues.Count; i++)
        {
            var color = GetRgbColorByValue(aggregatedValues[i]);
            payload += "\"" + color + "\",";
        }

        var rainSum = aggregatedValues.Select(q => q.Value).Sum();
        switch (rainSum)
        {
            case <= 0.3:
                _logger.LogInformation("☀️ So Sunny!");
                break;
            case <= 5:
                _logger.LogInformation("☔️ A bit rainy");
                break;
            case <= 10:
                _logger.LogInformation("⛈️ Bad weather");
                break;
            default:
                _logger.LogInformation("🏡 Better stay at home");
                break;
        }

        payload = payload[..^1];
        payload += "]}}\"";
        await PostPayload(payload);
    }
}
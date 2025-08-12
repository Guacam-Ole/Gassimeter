using System.Text;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GassiMeter;

public class Wled
{
    private readonly Config _config;
    private readonly Rest _rest;
    private const string ColorNiesel = "00B4FF";
    private const string ColorRegen = "0032C8";
    private const string ColorGewitter = "FF3296";
    private const string ColorSonne = "32FF32";
    private const string ColorHamburgerWetter = "DC1414";
    private const string ColorNope = "FFC800";


    public Wled(Config config, Rest rest)
    {
        _config = config;
        _rest = rest;
    }

    public async Task TurnOn()
    {
        Console.WriteLine($"💡 Turning WLED on with brightness '{_config.Wled.Brightness}'");
        var payload = "{\"on\":true,\"bri\":"+_config.Wled.Brightness+"} ";
        await PostPayload(payload);
    }

    public async Task TurnOff()
    {
        await ClearAllLeds();
        const string payload = "{\"off\":true } ";
        await PostPayload(payload)
            ;
        Console.WriteLine("🔌 LEDs Turned Off");
    }


    public async Task ClearAllLeds()
    {
        Console.WriteLine($"🧩 Clearing '{_config.Wled.Count}' LEDs");
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
        // TODO: REST
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

    private static string CalcGradientColor(Color minColor, double minValue, Color maxColor, double maxValue, double value)
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
                // Sonne
                return ColorSonne;
            case <= 0.5:
                // Niesel
                return ColorNiesel;
            case <= 2.5:
                // Regen
                return CalcGradientColor(ColorTranslator.FromHtml("#" + ColorNiesel), 0.5,
                    ColorTranslator.FromHtml("#" + ColorRegen),
                    2.5, value);
            case <= 5:
                // Gewitter
                return CalcGradientColor(ColorTranslator.FromHtml("#" + ColorRegen), 2.5,
                    ColorTranslator.FromHtml("#" + ColorGewitter),
                    5, value);
            case <= 10:
                // Hamburger Wetter
                return CalcGradientColor(ColorTranslator.FromHtml("#" + ColorGewitter), 5,
                    ColorTranslator.FromHtml("#" + ColorHamburgerWetter),
                    10, value);
            default:
            {
                // Unwetter
                if (value > 20) value = 20;
                return CalcGradientColor(ColorTranslator.FromHtml("#" + ColorHamburgerWetter), 10,
                    ColorTranslator.FromHtml("#" + ColorNope),
                    20, value);
            }
        }
    }


    public async Task SetLedsByValueJson(Dictionary<int, double> minuteValues)
    {
        // init aggregate:
        Dictionary<int, double> aggreatedValues = new();
        for (var i = 0; i <= _config.Wled.Count; i++)
        {
            aggreatedValues[i] = 0;
        }

        // minutevalues to ledvalues:
        foreach (var minuteValue in minuteValues)
        {
            var led = GetLedFromMinute(minuteValue.Key);
            if (led < 0 || led > _config.Wled.Count) continue; // should never happen
            aggreatedValues[led] += minuteValue.Value;
        }

        string payload = "{\"seg\":{\"i\":[";
        for (int i = 0; i < aggreatedValues.Count; i++)
        {
            var color = GetRgbColorByValue(aggreatedValues[i]);
            payload += "\"" + color + "\",";
        }

        payload = payload[..^1];
        payload += "]}}\"";
        await PostPayload(payload);

    }
}
using System.Text;

namespace GassiMeter;

public class Hass
{
    private readonly Rest _rest;
    private readonly Config _config;
    private readonly Secrets _secrets;

    public Hass(Rest rest, Config config, Secrets secrets)
    {
        _rest = rest;
        _config = config;
        _secrets = secrets;
    }

    public async Task<string?> GetSensorState(string sensor)
    {
        Console.WriteLine($"🏠 Requesting Home Assistant sensor data for '{sensor}'");
        var url = $"https://hass.oles.cloud/api/states/{_config.Hass.Sensor}";
        var entity = await _rest.Get<HassEntity>(url, _secrets.HassBearer);
        if (entity == null)
        {
            Console.WriteLine("⚠️ Failed to get Home Assistant sensor data");
        }
        return entity == null ? null : entity.State;
    }



    
    
}
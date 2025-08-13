using System.Text;
using Microsoft.Extensions.Logging;

namespace GassiMeter;

public class Hass
{
    private readonly Rest _rest;
    private readonly Config _config;
    private readonly Secrets _secrets;
    private readonly ILogger<Hass> _logger;

    public Hass(Rest rest, Config config, Secrets secrets, ILogger<Hass> logger)
    {
        _rest = rest;
        _config = config;
        _secrets = secrets;
        _logger = logger;
    }

    public async Task<string?> GetSensorState(string sensor)
    {
        _logger.LogInformation("🏠 Requesting Home Assistant sensor data for '{Sensor}'", sensor);
        var url = $"https://hass.oles.cloud/api/states/{sensor}";
        var entity = await _rest.Get<HassEntity>(url, _secrets.HassBearer);
        if (entity == null)
        {
            _logger.LogWarning("⚠️ Failed to get Home Assistant sensor data");
        }
        return entity == null ? null : entity.State;
    }



    
    
}
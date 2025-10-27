using Microsoft.Extensions.Logging;

namespace GassiMeter;

public class Looper
{
    private readonly OpenWeather _openWeather;
    private readonly Wled _wled;
    private readonly Hass _hass;
    private readonly History _history;
    private readonly Config _config;
    private readonly ILogger<Looper> _logger;

    public Looper(OpenWeather
        openWeather, Wled wled, Hass hass, History history, Config config, ILogger<Looper> logger)
    {
        _openWeather = openWeather;
        _wled = wled;
        _hass = hass;
        _history = history;
        _config = config;
        _logger = logger;
    }

    private async Task InitDisplay()
    {
        await _wled.TurnOff();
        await _wled.TurnOn();
        await _wled.ClearAllLeds();

        var allEntries = new Dictionary<int, double>();
        var state = await _wled.GetStatus<WledEntity>();
        _logger.LogInformation("💡 WLED Status: IsOn:'{IsOn}', Brightness: '{Brightness}'", state?.StateEntity.IsOn,
            state?.StateEntity.Brightness);
        for (var i = 0; i <= 60; i++)
        {
            double value = i % 10 == 0 ? 10 : 0;
            allEntries.Add(i, value);
        }

        await _wled.SetLedsByValueJson(allEntries);
        Thread.Sleep(TimeSpan.FromSeconds(10));
    }

    private async Task TurnWledOffIfOn()
    {
        try
        {
            var status = await _wled.GetStatus<WledEntity>();
            if (status is { StateEntity.IsOn: true })
            {
                await _wled.TurnOff();
            }
        }
        catch (Exception)
        {
            // assuming it is off
        }
    }

    private void SleepUntil(TimeSpan time) {
        var targetDate=DateTime.Today.AddDays(1).Add(time);
        var delay=targetDate-DateTime.Now;
        _logger.LogInformation("🛏️ Going to sleep for '{Timespan}'",delay);
        System.Threading.Thread.Sleep(delay);
        _logger.LogInformation("🌅 Awake again");
    }

    private async Task DisplayData()
    {
        // Check if 3D printer is running:
        var hassSensor = _config.Hass?.Sensor;
        if (hassSensor != null)
        {
            var hassSensorState = await _hass.GetSensorState(hassSensor);            
            if (hassSensorState != _config.Hass?.RequiredState)
            {
                _logger.LogWarning("❌ Wrong state for Home assistant Sensor '{Sensor}'. Will not continue", hassSensor);
                return;
            }
        }

        // Check Time:
        if (_config.OperationTime != null)
        {
            var now = DateTime.Now.ToLocalTime();
            var today = DateTime.Today.ToLocalTime();
            if (now < today.Add(_config.OperationTime.FromTime))
            {
                _logger.LogInformation("🛌 Still too early. Will not start before '{FromTime}'. It is now '{now}'.",
                    _config.OperationTime.FromTime, now);
                await TurnWledOffIfOn();
                SleepUntil(_config.OperationTime.FromTime);
                
                return;
            }

            if (now > today.Add(_config.OperationTime.ToTime))
            {
                _logger.LogInformation("🌙 Too late. Will not start before '{FromTime}' tomorrow. It is now '{now}'",
                    _config.OperationTime.FromTime, now);
                await TurnWledOffIfOn();
                SleepUntil(_config.OperationTime.FromTime);
                return;
            }
        }

        _logger.LogDebug("🌤️ Fetching weather data for coordinates: '{Latitude}', '{Longitude}'",
            _config.Weather.Latitude, _config.Weather.Longitude);
        var liveValues = await _openWeather.GetMinuteValues();
        if (liveValues?.Minutely == null)
        {
            _logger.LogWarning("🌦️ Could not receive weather data. Exiting");
            return;
        }

        // Store in History for next run
        liveValues.Minutely.ForEach(q => _history.AddHistoryData(q.Time, q.Rain));

        // Combine history and livedata:
        var liveAndHistoryValues = _history.GetHistoryData(_config.Wled.Start * _config.Wled.MinutesPerLed);
        foreach (var minuteValue in liveValues.Minutely)
        {
            var minute = (int)(minuteValue.Time - DateTime.Now).TotalMinutes;
            if (minute < 0) continue;
            liveAndHistoryValues.TryAdd(minute, minuteValue.Rain);
        }

        // send to Wled     
        await _wled.TurnOn();
        await _wled.SetLedsByValueJson(liveAndHistoryValues);
    }

    public async Task Loop()
    {
        _logger.LogInformation("🐕🦴️ Gassimeter started! 🌞");
        await InitDisplay();

        while (true)
        {
            try
            {                
                await DisplayData();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "💥 Error in main loop");
                // Let's hope this fixes itself in a few minutes
            }
            finally
            {
                Thread.Sleep(_config.Weather.Delay);
            }
        }
    }
}

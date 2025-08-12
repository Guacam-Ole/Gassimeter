namespace GassiMeter;

public class Looper
{
    private readonly OpenWeather _openWeather;
    private readonly Wled _wled;
    private readonly Hass _hass;
    private readonly History _history;
    private readonly Config _config;

    public Looper(OpenWeather
        openWeather, Wled wled, Hass hass, History history, Config config)
    {
        _openWeather = openWeather;
        _wled = wled;
        _hass = hass;
        _history = history;
        _config = config;
    }

    private async Task InitDisplay()
    {
        await _wled.TurnOff();
        await _wled.TurnOn();
        await _wled.ClearAllLeds();

        var allEntries = new Dictionary<int, double>();
        var state = await _wled.GetStatus<WledEntity>();
        Console.WriteLine(
            $"💡 WLED Status: IsOn:'{state?.StateEntity.IsOn}', Brightness: '{state?.StateEntity.Brightness}'");
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

    private async Task DisplayData()
    {
        // Check if 3D printer is running:
        var hassSensor = _config.Hass?.Sensor;
        if (hassSensor != null)
        {
            var hassSensorState = await _hass.GetSensorState(hassSensor);
            Console.WriteLine(
                $"🏠 Home Assistant sensor '{hassSensor}' state: '{hassSensorState}', required: '{_config.Hass?.RequiredState}'");
            if (hassSensorState != _config.Hass?.RequiredState)
            {
                Console.WriteLine($"❌ Wrong state for Home assistant Sensor '{hassSensor}'. Will not continue");
                return;
            }
        }

        // Check Time:
        if (_config.OperationTime != null)
        {
            if (DateTime.Now < DateTime.Today.Add(_config.OperationTime.FromTime))
            {
                Console.WriteLine($"😴 Still too early. Will not start before '{_config.OperationTime.FromTime}'");
                await TurnWledOffIfOn();
                return;
            }

            if (DateTime.Now > DateTime.Today.Add(_config.OperationTime.ToTime))
            {
                Console.WriteLine($"🌙 Too late. Will not start before '{_config.OperationTime.FromTime}' tomorrow");
                await TurnWledOffIfOn();
                return;
            }
        }

        Console.WriteLine(
            $"🌤️ Fetching weather data for coordinates: '{_config.Weather.Latitude}', '{_config.Weather.Longitude}'");
        var minuteValues = await _openWeather.GetMinuteValues();
        if (minuteValues?.Minutely == null)
        {
            Console.WriteLine("🌦️ Could not receive weather data. Exiting");
            return;
        }

        // Store in History for next run
        minuteValues.Minutely.ForEach(q => _history.AddHistoryData(q.Time, q.Rain));

        // Combine history and livedata:
        var allEntries = _history.GetHistoryData(_config.Wled.Start * _config.Wled.MinutesPerLed);
        foreach (var minuteValue in minuteValues.Minutely)
        {
            var minute = (int)(minuteValue.Time - DateTime.Now).TotalMinutes;
            if (minute < 0) continue;
            allEntries.TryAdd(minute, minuteValue.Rain);
        }

        // send to Wled
        Console.WriteLine($"🎨 Updating '{allEntries.Count}' LED values");
        await _wled.TurnOn();
        await _wled.SetLedsByValueJson(allEntries);
        Console.WriteLine($"✨ LED update complete!");
    }

    public async Task Loop()
    {
        Console.WriteLine("🐕🦴️ Gassimeter started! 🌞");
        await InitDisplay();

        while (true)
        {
            try
            {
                Console.WriteLine($"🔄 Starting Loop run at '{DateTime.Now}'");
                await DisplayData();
                Thread.Sleep(_config.Weather.Delay);
            }
            catch (Exception e)
            {
                Console.WriteLine($"💥 Error in main loop: '{e}'");
                // Let's hope this fixes itself in 2 min (sometimes wled gets issues)
            }
        }
    }
}
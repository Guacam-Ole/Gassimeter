# GassiMeter

A .NET application that monitors Home Assistant entities, checks weather conditions, and displays visual notifications on WLED-compatible LED strips.

## Overview

GassiMeter integrates three main components:
1. **Home Assistant Integration** (optional) - Monitor entity states
2. **Weather Monitoring** - Check rain forecasts via OpenWeather API
3. **LED Visualization** - Display information on WLED-compatible LED strips

The application runs continuously, checking conditions at configurable intervals and updating LED displays accordingly.

## Requirements

- .NET 9.0 or later
- WLED-compatible LED strip with ESP32
- OpenWeather API key (free tier available)
- Home Assistant instance (optional)

## Configuration

### config.json

The application includes a `config.json` file that needs to be configured with your specific settings:

**Hass (Optional)**
- `Sensor`: The Home Assistant entity ID to monitor
- `RequiredState`: The required state value to check for

**Weather (Required)**
- `Longitude`: Geographic longitude for weather data
- `Latitude`: Geographic latitude for weather data  
- `Delay`: Interval between weather checks (format: "HH:MM:SS", default: "00:02:00")

**Wled (Required)**
- `Url`: WLED device JSON API endpoint
- `Count`: Number of LEDs to control (default: 60)
- `Start`: Starting LED index (default: 0)
- `MinutesPerLed`: Minutes represented per LED (default: 2)
- `Brightness`: LED brightness level 0-255 (default: 128)

**OperationTime (Optional)**
- `FromTime`: Start time for operation (format: "HH:MM:SS", e.g., "06:00:00")
- `ToTime`: End time for operation (format: "HH:MM:SS", e.g., "22:00:00")

### secrets.json

1. Copy `secrets.example.json` to `secrets.json`
2. Fill in your actual values:

- `HassBearer`: Home Assistant long-lived access token (only required if using Hass integration)
- `WeatherApiKey`: OpenWeather API key (required)

## API Usage and Rate Limits

### OpenWeather API

The application uses the OpenWeather API to fetch hourly rain forecasts. The free tier includes:
- **1,000 API calls per day**
- Rate limit considerations for continuous operation

**Important**: With the default 2-minute interval, running 24/7 will make approximately 720 calls per day, which is within the free tier limit. However, to stay well within limits and account for any retries or additional calls, consider using an interval of 2 minutes or higher.

## Getting Started

1. Clone the repository
2. Install .NET 9.0 SDK
3. Configure `config.json` with your location and device settings
4. Copy `secrets.example.json` to `secrets.json` and add your API keys
5. Get your OpenWeather API key from [openweathermap.org](https://openweathermap.org/api)
6. Configure your WLED device and note its IP address
7. Build and run the application:

```bash
dotnet build
dotnet run
```

## Home Assistant Integration

To use the Home Assistant integration:

1. Generate a long-lived access token in Home Assistant
2. Add the token to `secrets.json` as `HassBearer`
3. Configure the `Hass` section in `config.json` with your entity and required state
4. If the Hass configuration is omitted, this step will be skipped automatically

## WLED Setup

1. Flash your ESP32 with WLED firmware
2. Connect to your WiFi network
3. Note the device's IP address
4. Update the `Wled.Url` in your config to point to `http://[device-ip]/json/state`

## Troubleshooting

- **Configuration errors**: Ensure JSON syntax is valid in both config files
- **API errors**: Verify your OpenWeather API key is valid and active
- **WLED connection**: Check that the ESP32 is accessible on your network
- **Home Assistant**: Verify your long-lived token has appropriate permissions

## License

This project is open source. Please check the LICENSE file for details.
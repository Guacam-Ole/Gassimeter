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

## 3D Printed files
For the legend (show what color means what) and scale I created a few STL files as well as the Blend File (for Blender) so you can change the text if you decide to use my design:

https://www.thingiverse.com/thing:7117297

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
- `Colors`: Array of color-to-rain-amount mappings for LED display
  - `ColorCode`: 6-character hex color code (e.g., "32FF32" for green)
  - `RainAmount`: Rain amount in mm/hour that triggers this color
  - **Required values**: Must include entries for `RainAmount: -1` (missing data) and `RainAmount: 0` (no rain)
  - Colors are interpolated between defined values for smooth gradients
  - Example color scheme:
    - `-1`: "000000" (Black - missing historical data)
    - `0`: "32FF32" (Green - sunny/no rain)
    - `0.5`: "00B4FF" (Light Blue - drizzle)
    - `2.5`: "0032C8" (Blue - light rain)
    - `5`: "FF3296" (Pink - heavy rain/thunderstorm)
    - `10`: "DC1414" (Red - very heavy rain)
    - `20`: "FFC800" (Orange - extreme weather)

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

### Option 1: Running with .NET

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

### Option 2: Running with Docker

1. Clone the repository
2. Configure `config.json` and `secrets.json` on your host machine
3. Build the Docker image:

```bash
docker build -t gassimeter .
```

4. Run the container with volume mounts for configuration:

```bash
docker run -d \
  --name gassimeter \
  -v /path/to/your/config.json:/app/config.json \
  -v /path/to/your/secrets.json:/app/secrets.json \
  gassimeter
```

Replace `/path/to/your/` with the actual paths to your configuration files on the host system.

#### Docker Volume Configuration

The application requires two configuration files that should be mounted as volumes:

- **config.json**: Application settings including weather location, WLED device URL, and color mappings
- **secrets.json**: API keys and sensitive credentials

Example with absolute paths:
```bash
docker run -d \
  --name gassimeter \
  -v /home/user/gassimeter/config.json:/app/config.json \
  -v /home/user/gassimeter/secrets.json:/app/secrets.json \
  gassimeter
```

This approach keeps your configuration and secrets outside the container, making it easy to update settings without rebuilding the image.

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

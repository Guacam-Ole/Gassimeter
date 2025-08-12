using System.Text.Json;
using GassiMeter;
using Microsoft.Extensions.DependencyInjection;

internal static class Program
{
    public static void Main()
    {
        var services = CreateServiceProvider();

        var looper = services.GetRequiredService<Looper>();
        looper.Loop().Wait();
    }

    private static T FromJson<T>(string filename)
    {
        var json = File.ReadAllText(filename);
        var options = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        return JsonSerializer.Deserialize<T>(json, options) ?? throw new InvalidOperationException($"Cannot deserialize '{filename}'");
    }
    
    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(FromJson<Config>("config.json")); 
        services.AddSingleton(FromJson<Secrets>("secrets.json"));
        services.AddScoped<Hass>();
        services.AddScoped<Rest>();
        services.AddScoped<OpenWeather>();
        services.AddScoped<Looper>();
        services.AddSingleton<History>();
        
        services.AddScoped<Wled>();

        return services.BuildServiceProvider();
    }
}



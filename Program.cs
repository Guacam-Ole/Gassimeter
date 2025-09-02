using System.Reflection;
using System.Text.Json;
using GassiMeter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Grafana.Loki;

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
        
        services.AddLogging(cfg => cfg.SetMinimumLevel(LogLevel.Debug));
        services.AddSerilog(cfg =>
        {
            cfg.MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("job", Assembly.GetEntryAssembly()?.GetName().Name)
                .Enrich.WithProperty("service", Assembly.GetEntryAssembly()?.GetName().Name)
                .Enrich.WithProperty("desktop", Environment.GetEnvironmentVariable("DESKTOP_SESSION"))
                .Enrich.WithProperty("language", Environment.GetEnvironmentVariable("LANGUAGE"))
                .Enrich.WithProperty("lc", Environment.GetEnvironmentVariable("LC_NAME"))
                .Enrich.WithProperty("timezone", Environment.GetEnvironmentVariable("TZ"))
                .Enrich.WithProperty("dotnetVersion", Environment.GetEnvironmentVariable("DOTNET_VERSION"))
                .Enrich.WithProperty("inContainer",
                    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"))
                .WriteTo.GrafanaLoki(Environment.GetEnvironmentVariable("LOKIURL") ?? "http://thebeast:3100",
                    propertiesAsLabels: ["job"]);
            if (Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ==
                "Debug")
            {
                cfg.WriteTo.Console(new RenderedCompactJsonFormatter());
            }
        });
        
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




using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace GassiMeter;

public class Rest
{
   private readonly ILogger<Rest> _logger;

   public Rest(ILogger<Rest> logger)
   {
      _logger = logger;
   }

   public async Task<T?> Get<T>(string url, string? bearer=null)
   {
      try
      {
         using var httpClient = new HttpClient();
         httpClient.DefaultRequestHeaders.Clear();
         if (bearer!=null) httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearer}");
         var response = await httpClient.GetStringAsync(url);
         return JsonSerializer.Deserialize<T?>(response);
      }
      catch (Exception e)
      {
         _logger.LogError(e, "HTTP request failed for URL: {Url}", url);
         return default;
      }
   }
}
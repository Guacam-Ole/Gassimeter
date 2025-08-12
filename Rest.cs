
using System.Text.Json;

namespace GassiMeter;

public class Rest
{
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
         Console.WriteLine(e);
         return default;
      }
   }
}
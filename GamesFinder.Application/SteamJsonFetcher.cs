using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GamesFinder.Application;

public class SteamJsonFetcher
{
    private static readonly HttpClient Client = new();
    private readonly ILogger<SteamJsonFetcher> _logger;
    private readonly SteamOptions _options;

    public SteamJsonFetcher(ILogger<SteamJsonFetcher> logger, SteamOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task<bool> Update()
    {
        var key = _options.ApiKey;
        var response = await Client.GetAsync(new Uri($"http://api.steampowered.com/ISteamApps/GetAppList/v0002/?key={key}&format=json"));
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("SteamJsonFetcher failed to get app list");
            return false;
        }
        
        var json = await response.Content.ReadAsStringAsync();
        var jObj = JsonConvert.DeserializeObject<JObject>(json)?["applist"];
        if (jObj == null)
        {
            _logger.LogError("SteamJsonFetcher failed to get app list");
            return false;
        }
        
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "applist.json");
        await File.WriteAllTextAsync(filePath, jObj.ToString(Formatting.Indented), Encoding.UTF8);
        return true;
    }
}
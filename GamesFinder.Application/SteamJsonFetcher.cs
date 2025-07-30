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
    private string _filePath;

    public SteamJsonFetcher(ILogger<SteamJsonFetcher> logger, SteamOptions options)
    {
        _logger = logger;
        _options = options;
        _filePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "applist.json");;
    }

    public async Task<( DateTime, int )?> GetMetadata()
    {
        try
        {
            var fileInfo = new FileInfo(_filePath);
            var apps = (JArray) JObject.Parse(await File.ReadAllTextAsync(_filePath))["apps"]!;
            
            return (fileInfo.LastWriteTime, apps.Count);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task<bool> Update()
    {
        var key = _options.ApiKey;
        var response = await Client.GetAsync(new Uri("http://api.steampowered.com/ISteamApps/GetAppList/v2/"));
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
        
        await File.WriteAllTextAsync(_filePath, jObj.ToString(Formatting.Indented), Encoding.UTF8);
        return true;
    }
}

using System.Net;
using GamesFinder.Domain.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GamesFinder.Domain.Crawlers;

public class SteamCrawler : ICrawler
{
    private static readonly HttpClient Client = new HttpClient();
    private static readonly string GameData = "https://store.steampowered.com/api/appdetails?appids={appId}";
    private List<Game> Games = new();
    private string UserLink;
    private int callsCount = 0;
    
    public SteamCrawler(string userId, string sessionToken)
    {
        var cookies = new CookieContainer();
        cookies.Add(
            new Uri("https://store.steampowered.com"),
            new Cookie("steamLoginSecure", sessionToken)
            );
        var handler = new HttpClientHandler
        {
            CookieContainer = cookies,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; Crawler/1.0)");
        UserLink = $"https://store.steampowered.com/dynamicstore/userdata/?id={userId}";
    }

    public async Task CrawlAsync()
    {
        var json = await Client.GetStringAsync(UserLink);
        callsCount++;
        var userData = JsonConvert.DeserializeObject<JObject>(json);
        Console.WriteLine(userData);
        var wishlist = userData["rgWishlist"] as JObject;
        if (wishlist == null)
        {
            Console.WriteLine("No wishlist found");
            return;
        }
        foreach (var prop in wishlist.Properties())
        {
            string appId = prop.Name;
            await ExtractGame(appId);
        }
    }

    // public async Task<Game> ExtractGame(string appId)
    public async Task ExtractGame(string appId)
    {
        if (callsCount == 200)
        {
            await Task.Delay(TimeSpan.FromMinutes(5));
        }
        var json = await Client.GetStringAsync(GameData.Replace("{appId}", appId));
        var gameData = JsonConvert.DeserializeObject<JObject>(json);
        Console.WriteLine(gameData);
        callsCount++;
    }
}
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Crawlers;
using GamesFinder.Domain.Entities;
using GamesFinder.Domain.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GamesFinder.Application;

public class SteamCrawler : ISteamCrawler
{
    private static readonly HttpClient Client = new HttpClient();
    private static readonly string GameData = "https://store.steampowered.com/api/appdetails?appids=";
    private static readonly int MaxReqs = 200;
    private static readonly int CooldownMinutes = 5;
    private readonly ILogger<SteamCrawler> _logger;

    public SteamCrawler(ILogger<SteamCrawler> logger)
    {
        _logger = logger;
    }

    public async Task<List<Game>> CrawlGamesAsync(ICollection<int> gameIds)
    {
        var games = new List<Game>();
        var callsCount = 0;

        foreach (int gameId in gameIds)
        {
            if (callsCount != 0 && callsCount % 200 == 0)
            {
                await Task.Delay(TimeSpan.FromMinutes(CooldownMinutes));
            }
            
            var response = await Client.GetAsync(new Uri(GameData + gameId));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error getting game data for {gameId}, ${response.StatusCode}");
                continue;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            
            Game? game = await ExtractGame(gameId, json);
            if (game == null) continue;
            
            games.Add(game);

            callsCount++;
        }
        
        return games;
    }
    
    public async Task<List<Game>> CrawlPricesAsync(ICollection<Game> games)
    {
        var callsCount = 0;
        throw new NotImplementedException("Implement");
    }

    public async Task<Game?> ExtractGame(int appId, string json)
    {
        var jObj = JsonConvert.DeserializeObject<JObject>(json)?[$"{appId}"];
        if (jObj == null)
        {
            _logger.LogError($"Failed to parse json: {json}");
            return null;
        }

        string? name = jObj["data"]?["name"]?.ToString();
        string? description = jObj["data"]?["detailed_description"]?.ToString();
        string? currency = jObj["data"]?["price_overview"]?["currency"]?.ToString();
        string? initialPrice = jObj["data"]?["price_overview"]?["initial"]?.ToString() ?? "0";
        string? currentPrice = jObj["data"]?["price_overview"]?["final"]?.ToString();
        string steamUrl = $"https://store.steampowered.com/app/{appId}";

        if (name == null) return null;
        var game = new Game(name: name, description: description, appId: appId, steamURL: steamUrl);

        Dictionary<ECurrency, (decimal, decimal)> prices = new();
        switch (currency)
        {
            case null:
                prices.Add(ECurrency.Eur, (0, 0));
                break;
            case "EUR":
                prices.Add(ECurrency.Eur, (decimal.Parse(initialPrice), decimal.Parse(currentPrice ?? initialPrice)));
                break;
            case "USD":
                prices.Add(ECurrency.Usd, (decimal.Parse(initialPrice), decimal.Parse(currentPrice ?? initialPrice)));
                break;
        }
        
        
        game.Offers.Add(new GameOffer(gameId: game.Id!, vendor: "Steam", prices: prices, available: true));
        
        return game;
    }
}
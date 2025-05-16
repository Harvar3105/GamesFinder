using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;
using GamesFinder.Domain.Interfaces.Crawlers;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GamesFinder.Application;

public class SteamCrawler : ISteamCrawler
{
    private static readonly HttpClient Client = new();
    private static readonly string GameData = "https://store.steampowered.com/api/appdetails?appids=";
    private static readonly int MaxReqs = 200;
    private static readonly int CooldownMinutes = 5;
    private readonly ILogger<SteamCrawler> _logger;
    private readonly IGameOfferRepository<GameOffer> _gameOfferRepository;
    private readonly IGameRepository<Game> _gameRepository;

    public SteamCrawler(ILogger<SteamCrawler> logger, IGameRepository<Game> gameRepository, IGameOfferRepository<GameOffer> gameOfferRepository)
    {
        _logger = logger;
        _gameRepository = gameRepository;
        _gameOfferRepository = gameOfferRepository;
    }

    public async Task CrawlGamesAsync(ICollection<int> gameIds, bool force = false)
    {
        var games = new List<Game>();
        var callsCount = 0;

        foreach (int gameId in gameIds)
        {
            if (callsCount != 0 && callsCount % MaxReqs == 0)
            {
                await SaveOrUpdateBulk(games);
                await Task.Delay(TimeSpan.FromMinutes(CooldownMinutes));
            }

            if (!force)
            {
                if (await _gameRepository.ExistsByAppIdAsync(gameId)) continue;
            }
            
            var response = await Client.GetAsync(new Uri(GameData + gameId));
            callsCount++;
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error getting game data for {gameId}, ${response.StatusCode}");
                continue;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            
            Game? game = ExtractGame(gameId, json);
            if (game == null) continue;
            
            games.Add(game);
        }
        
        await SaveOrUpdateBulk(games);
    }

    private async Task SaveOrUpdateBulk(List<Game> games)
    {
        List<GameOffer> gamesOffers = new();
        foreach (var game in games)
        {
            gamesOffers.AddRange(game.Offers);
        }
        
        var success1 = await _gameRepository.SaveManyAsync(games);
        if (!success1)
        {
            _logger.LogError("Something went wrong, couldn't save games");
        }
            
        var success2 = await _gameOfferRepository.SaveManyAsync(gamesOffers);
        if (!success2)
        {
            _logger.LogError("Something went wrong, couldn't save games");
        }
        
        games.Clear();
    }
    
    public async Task CrawlPricesAsync(ICollection<Game> games, bool force = false)
    {
        var callsCount = 0;
        throw new NotImplementedException("Implement");
    }

    private Game? ExtractGame(int appId, string json)
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

        Dictionary<ECurrency, GameOffer.PriceRange> prices = new();
        switch (currency)
        {
            case null:
                prices.Add(ECurrency.Eur, new GameOffer.PriceRange(null, null));
                break;
            case "EUR":
                prices.Add(ECurrency.Eur, new GameOffer.PriceRange(decimal.Parse(initialPrice), decimal.Parse(currentPrice ?? initialPrice)));
                break;
            case "USD":
                prices.Add(ECurrency.Usd, new GameOffer.PriceRange(decimal.Parse(initialPrice), decimal.Parse(currentPrice ?? initialPrice)));
                break;
        }
        
        
        game.Offers.Add(new GameOffer(gameId: game.Id!, vendor: "Steam", prices: prices, available: true));
        
        return game;
    }
}
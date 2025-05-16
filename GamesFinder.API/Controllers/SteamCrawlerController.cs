using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Crawlers;
using GamesFinder.Domain.Entities;
using GamesFinder.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesFinder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SteamCrawlerController : ControllerBase
{
    private readonly ISteamCrawler _steamCrawlerController;
    private readonly IGameOfferRepository<GameOffer> _gameOfferRepository;
    private readonly IGameRepository<Game> _gameRepository;
    private readonly ILogger<SteamCrawlerController> _logger;

    public SteamCrawlerController(ISteamCrawler steamCrawlerController, IGameOfferRepository<GameOffer> gameOfferRepository, IGameRepository<Game> gameRepository, ILogger<SteamCrawlerController> logger)
    {
        _steamCrawlerController = steamCrawlerController;
        _gameOfferRepository = gameOfferRepository;
        _gameRepository = gameRepository;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CrawlSteam([FromBody] ICollection<int> gamesIds)
    {
        _logger.LogInformation("Crawling games...");
        
        int batches = (int)Math.Ceiling(gamesIds.Count / (decimal)200);
        int pauses = Math.Max(0, batches - 1);
        int totalCooldownMinutes = pauses * 5;

        _ = Task.Run(async () =>
        {
            List<Game> games = await _steamCrawlerController.CrawlGamesAsync(gamesIds);
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
        });
        
        _logger.LogInformation("Crawling finished");
        return Accepted($"Crawling started, will take around {totalCooldownMinutes} minutes");
    }
}
using GamesFinder.Application;
using GamesFinder.Application.Crawlers;
using GamesFinder.Domain.Interfaces.Crawlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesFinder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SteamCrawlerController : ControllerBase
{
    private readonly ICrawler _steamCrawler;
    private readonly ILogger<SteamCrawlerController> _logger;
    private readonly SteamJsonFetcher _steamJsonFetcher;
    private readonly GameSteamAppIdFinder _gameSteamAppIdFinder;

    public SteamCrawlerController(SteamCrawler steamCrawler, ILogger<SteamCrawlerController> logger, SteamJsonFetcher steamJsonFetcher, GameSteamAppIdFinder gameSteamAppIdFinder)
    {
        _steamCrawler = steamCrawler;
        _steamJsonFetcher = steamJsonFetcher;
        _gameSteamAppIdFinder = gameSteamAppIdFinder;
        _logger = logger;
    }

    [HttpPost("AppList")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAllGamesJson()
    {
        var success = await _steamJsonFetcher.Update();
        if (!success)
        {
            return StatusCode(500, "Failed to fetch game list.");
        }
        
        _gameSteamAppIdFinder.Update();
        
        return Accepted();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CrawlSteam([FromBody] CrawlerControllerModel request)
    {
        _logger.LogInformation("Crawling Steam games...");
        
        var gamesIds = request.GamesIds!;
        if (gamesIds == null)
        {
            _logger.LogCritical("Cannot crawl all games!");
            return StatusCode(500, "Cannot crawl all games!");
        }
        
        int batches = (int)Math.Ceiling(gamesIds.Count / (decimal)200);
        int pauses = Math.Max(0, batches - 1);
        int totalCooldownMinutes = pauses * 5;

        _ = Task.Run(async () =>
        {
            await _steamCrawler.CrawlGamesAsync(gamesIds, request.ForceUpdate);
            _logger.LogInformation("Crawling Steam finished");
        });
        
        return Accepted($"Crawling started, will take around {totalCooldownMinutes} minutes");
    }
}

public class CrawlerControllerModel
{
    public List<int>? GamesIds { get; set; }
    public bool ForceUpdate { get; set; } = false;
}
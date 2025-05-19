using GamesFinder.Application;
using GamesFinder.Domain.Interfaces.Crawlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesFinder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SteamCrawlerController : ControllerBase
{
    private readonly ICrawler _steamCrawlerController;
    private readonly ILogger<SteamCrawlerController> _logger;
    private readonly SteamJsonFetcher _steamJsonFetcher;
    private readonly GameSteamAppIdFiner _gameSteamAppIdFiner;

    public SteamCrawlerController(ICrawler steamCrawlerController, ILogger<SteamCrawlerController> logger, SteamJsonFetcher steamJsonFetcher, GameSteamAppIdFiner gameSteamAppIdFiner)
    {
        _steamCrawlerController = steamCrawlerController;
        _steamJsonFetcher = steamJsonFetcher;
        _gameSteamAppIdFiner = gameSteamAppIdFiner;
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
        
        _gameSteamAppIdFiner.Update();
        
        return Ok();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CrawlSteam([FromBody] SteamCrawlerControllerModel request)
    {
        _logger.LogInformation("Crawling games...");
        
        var gamesIds = request.GamesIds;
        
        int batches = (int)Math.Ceiling(gamesIds.Count / (decimal)200);
        int pauses = Math.Max(0, batches - 1);
        int totalCooldownMinutes = pauses * 5;

        _ = Task.Run(async () =>
        {
            await _steamCrawlerController.CrawlGamesAsync(gamesIds, request.ForceUpdate);
            _logger.LogInformation("Crawling finished");
        });
        
        return Accepted($"Crawling started, will take around {totalCooldownMinutes} minutes");
    }
}

public class SteamCrawlerControllerModel
{
    public required List<int> GamesIds { get; set; }
    public bool ForceUpdate { get; set; } = false;
}
using GamesFinder.Domain.Interfaces.Crawlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesFinder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SteamCrawlerController : ControllerBase
{
    private readonly ISteamCrawler _steamCrawlerController;
    private readonly ILogger<SteamCrawlerController> _logger;

    public SteamCrawlerController(ISteamCrawler steamCrawlerController, ILogger<SteamCrawlerController> logger)
    {
        _steamCrawlerController = steamCrawlerController;

        _logger = logger;
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
            await _steamCrawlerController.CrawlGamesAsync(gamesIds);
            _logger.LogInformation("Crawling finished");
        });
        
        return Accepted($"Crawling started, will take around {totalCooldownMinutes} minutes");
    }
}

public class SteamCrawlerControllerModel
{
    public List<int> GamesIds { get; set; }
    public bool ForceUpdate { get; set; } = false;
}
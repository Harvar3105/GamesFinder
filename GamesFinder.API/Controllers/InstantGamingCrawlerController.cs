using GamesFinder.Application;
using GamesFinder.Application.Crawlers;
using GamesFinder.Domain.Interfaces.Crawlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesFinder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstantGamingCrawlerController : ControllerBase
{
    private readonly InstantGamingCrawler _instantGamingCrawlerController;
    private readonly ILogger<InstantGamingCrawlerController> _logger;

    public InstantGamingCrawlerController(InstantGamingCrawler instantGamingCrawler, ILogger<InstantGamingCrawlerController> logger)
    {
        _instantGamingCrawlerController = instantGamingCrawler;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CrawlInstantGaming([FromBody] CrawlerControllerModel model)
    {
        _logger.LogInformation("Crawling IG games...");

        _ = Task.Run(async () =>
        {
            await _instantGamingCrawlerController.CrawlGamesAsync(model.GamesIds, model.ForceUpdate);
            _logger.LogInformation("Crawling finished");
        });
            
        return Accepted($"Crawling started!");
    }

    [HttpPost("All")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CrawlEntireInstantGaming([FromBody] InstantGamingCrawlerModel model)
    {
        _logger.LogInformation("Crawling Instant Gaming games...");

        if (model.maxCalls == 0) return StatusCode(404, "Not enough calls!");

        _ = Task.Run(async () =>
        {
            await _instantGamingCrawlerController.CrawlAllGamesAsync(model.maxCalls, model.forceUpdate);
            _logger.LogInformation("Crawling Instant Gaming finished");
        });
        
        return Accepted($"Crawling started!");
    }
}

public class InstantGamingCrawlerModel
{
    public int maxCalls { get; set; }
    public bool forceUpdate { get; set; } = false;
}

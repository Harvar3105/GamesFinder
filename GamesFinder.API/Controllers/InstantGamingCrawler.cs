using GamesFinder.Domain.Interfaces.Crawlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesFinder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstantGamingCrawler : ControllerBase
{
    private readonly ICrawler _InstantGamingCrawler;
    private readonly ILogger<InstantGamingCrawler> _logger;

    public InstantGamingCrawler(ICrawler instantGamingCrawler, ILogger<InstantGamingCrawler> logger)
    {
        _InstantGamingCrawler = instantGamingCrawler;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CrawlInstantGaming([FromBody] CrawlerControllerModel model)
    {
        _logger.LogInformation("Crawling IG games...");

        _ = Task.Run(async () =>
        {
            await _InstantGamingCrawler.CrawlGamesAsync(model.GamesIds, model.ForceUpdate);
            _logger.LogInformation("Crawling finished");
        });
            
        return Accepted($"Crawling started!");
    }
}

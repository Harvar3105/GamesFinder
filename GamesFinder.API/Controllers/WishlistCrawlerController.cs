using GamesFinder.Domain.Crawlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesFinder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WishlistCrawlerController : ControllerBase
{
    [HttpGet("{userId}")]
    [Authorize]
    public async Task<IActionResult> CrawlWishlist(ICollection<string> gamesIds)
    {
        // var crawler = new SteamCrawler(userId);
        // await crawler.CrawlAsync();
        return Ok("Crawling completed");
    }
}
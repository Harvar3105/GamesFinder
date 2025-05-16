using GamesFinder.Domain.Classes.Entities;

namespace GamesFinder.Domain.Interfaces.Crawlers;

public interface ISteamCrawler
{
    public Task CrawlGamesAsync(ICollection<int> gameIds);
    public Task CrawlPricesAsync(ICollection<Game> games);
}
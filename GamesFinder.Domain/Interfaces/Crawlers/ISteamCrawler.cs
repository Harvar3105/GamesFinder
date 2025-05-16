using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Entities;

namespace GamesFinder.Domain.Crawlers;

public interface ISteamCrawler
{
    public Task CrawlGamesAsync(ICollection<int> gameIds);
    public Task CrawlPricesAsync(ICollection<Game> games);
}
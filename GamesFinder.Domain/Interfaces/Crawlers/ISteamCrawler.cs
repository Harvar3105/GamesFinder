using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Entities;

namespace GamesFinder.Domain.Crawlers;

public interface ISteamCrawler
{
    public Task<List<Game>> CrawlGamesAsync(ICollection<int> gameIds);
    public Task<List<Game>> CrawlPricesAsync(ICollection<Game> games);
}
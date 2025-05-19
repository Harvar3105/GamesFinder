using GamesFinder.Domain.Classes.Entities;

namespace GamesFinder.Domain.Interfaces.Crawlers;

public interface ICrawler
{
    public Task CrawlGamesAsync(ICollection<int>? gameIds, bool force = false);
    public Task CrawlPricesAsync(ICollection<Game>? games, bool force = false);
}
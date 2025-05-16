using GamesFinder.Domain.Entities;

namespace GamesFinder.Domain.Crawlers;

public interface ICrawler
{
    Task CrawlAsync();
}
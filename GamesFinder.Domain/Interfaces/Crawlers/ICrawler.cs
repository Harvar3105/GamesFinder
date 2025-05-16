namespace GamesFinder.Domain.Interfaces.Crawlers;

public interface ICrawler
{
    Task CrawlAsync();
}
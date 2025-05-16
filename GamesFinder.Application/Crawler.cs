using AngleSharp.Html.Parser;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Crawlers;
using GamesFinder.Domain.Entities;
using GamesFinder.Domain.Enums;

namespace GamesFinder.Application;

public class Crawler : ICrawler
{
    private static readonly HttpClient Client = new HttpClient();
    private static readonly HtmlParser HtmlParser = new HtmlParser();
    private readonly string Vendor;
    private readonly string Url;
    private readonly ECurrency Currency;
    private readonly int StartId;
    private readonly int StopId;
    private readonly decimal Delay;
    private int CurrentId { get; set; }
    private Dictionary<Game, List<GameOffer>> Offers { get; } = new ();

    public Crawler(string vendor, int startId, int stopId, decimal? delay, ECurrency currency)
    {
        Vendor = vendor;
        StartId = startId;
        CurrentId = startId;
        StopId = stopId;
        Delay = delay ?? 0;
        Currency = currency;
    }

    public async Task CrawlAsync()
    {
        try
        {
            while (CurrentId <= StopId)
            {
                var pageContent = await Client.GetStringAsync(Url);
                var game = ExtractGame(pageContent);
                
                
                CurrentId++;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching: {e.Message}");
        }
    }

    private Game ExtractGame(string pageContent)
    {
        var document = HtmlParser.ParseDocument(pageContent);
        var name = document.QuerySelector(".game-title")?.TextContent;
        var description = document.QuerySelector(".text.readable")?.TextContent;
        
        if (name == null) throw new NullReferenceException("Game title not found");

        return new Game(name: name, description: description);
    }
    
}
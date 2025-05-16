using GamesFinder.Domain.Entities;
using GamesFinder.Domain.Enums;
using AngleSharp;
using AngleSharp.Html.Parser;

namespace GamesFinder.Domain.Crawlers;

public class Crawler : ICrawler
{
    private static readonly HttpClient Client = new HttpClient();
    private static readonly HtmlParser HtmlParser = new HtmlParser();
    private readonly Vendor Vendor;
    private readonly ECurrency Currency;
    private readonly int StartId;
    private readonly int StopId;
    private readonly decimal Delay;
    private int CurrentId { get; set; }
    private Dictionary<Game, List<GameOffer>> Offers { get; } = new ();

    public Crawler(Vendor vendor, int startId, int stopId, decimal? delay, ECurrency currency)
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
                var pageContent = await Client.GetStringAsync(FormNewString());
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

    private String FormNewString()
    {
        return Vendor.URL.Replace("{lang}", "en").Replace("{id}", CurrentId.ToString());
    }
}
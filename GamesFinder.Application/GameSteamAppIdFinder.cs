using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace GamesFinder.Application;

public class GameSteamAppIdFinder
{
    private static readonly string pathToFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "applist.json");
    private JObject? appsIds = null;
    private readonly ILogger<GameSteamAppIdFinder> logger;

    public GameSteamAppIdFinder(ILogger<GameSteamAppIdFinder> logger)
    {
        this.logger = logger;
        
        Update();
    }

    public void Update()
    {
        try
        {
            appsIds = JObject.Parse(File.ReadAllText(pathToFile));
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }

    public UnprocessedGame? FindApp(string gameName)
    {
        if (appsIds == null)
        {
            logger.LogError("File not found!");
            return null;
        }
        
        JArray apps = (JArray) appsIds["apps"]!;
        JObject? found = apps
            .FirstOrDefault(app => string.Equals((string)app["name"]!, gameName, StringComparison.OrdinalIgnoreCase)) as JObject;

        if (found == null)
        {
            found = apps
                .FirstOrDefault(app => string.Equals((string)app["name"]!, "Expansion - " + gameName, StringComparison.OrdinalIgnoreCase)) as JObject;

            if (found == null)
            {
                logger.LogInformation($"Not found for {gameName}");
                return null;
            }
        }

        var steamId = found["appid"]?.ToString();
        var steamName = found["name"]?.ToString();

        if (steamId == null || steamName == null) return null;
        return new UnprocessedGame(
            vendorsName: gameName,
            steamId: int.Parse(steamId),
            steamName: steamName
            );
    }
}
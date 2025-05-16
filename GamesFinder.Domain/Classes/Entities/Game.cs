using GamesFinder.Domain.Enums;

namespace GamesFinder.Domain.Entities;

public class Game : Entity
{
    public string Name { get; }
    public string? SteamURL { get; set; }
    public int? AppId { get; set; }
    public string? Description { get; }
    public IReadOnlyCollection<GameOffer> Offers { get; }

    public Game(string name, Guid? id = null, List<GameOffer>? initialOffers = null, string? description = null, string? steamURL = null, int? appId = null)
    {
        Id = id;
        Name = name;
        Description = description;
        Offers = initialOffers ?? new List<GameOffer>();
        SteamURL = steamURL;
        AppId = appId;
    }
    
    public void AddOffer(GameOffer offer) => ((List<GameOffer>)Offers).Add(offer);
}
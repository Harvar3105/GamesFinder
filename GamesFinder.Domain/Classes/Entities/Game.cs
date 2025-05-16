using GamesFinder.Domain.Entities;
using MongoDB.Bson.Serialization.Attributes;

namespace GamesFinder.Domain.Classes.Entities;

public class Game : Entity
{
    [BsonElement("name")]
    public string Name { get; set; }
    
    [BsonElement("steam_url")]
    public string? SteamURL { get; set; }
    [BsonElement("app_id")]
    public int? AppId { get; set; }
    [BsonElement("description")]
    public string? Description { get; set; }
    [BsonIgnore]
    public List<GameOffer> Offers;

    public Game(string name, List<GameOffer>? initialOffers = null, string? description = null, string? steamURL = null, int? appId = null)
    {
        Name = name;
        Description = description;
        Offers = initialOffers ?? new List<GameOffer>();
        SteamURL = steamURL;
        AppId = appId;
    }
}
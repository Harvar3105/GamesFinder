using GamesFinder.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GamesFinder.Domain.Classes.Entities;

public class Game : Entity
{
    [BsonElement("name")]
    public string Name { get; set; }
    
    [BsonElement("simplified_name")]
    public string SimplifiedName { get; set; }

    [BsonElement("steam_url")]
    public string? SteamURL { get; set; }
    [BsonElement("game_ids")]
    public List<GameId> GameIds { get; set; } = new();
    [BsonElement("description")]
    public string? Description { get; set; }
    [BsonElement("header_image")]
    public string? HeaderImage { get; set; }
    [BsonIgnore]
    public List<GameOffer> Offers = new();

    public Game(
        string name,
        List<GameOffer>? initialOffers = null,
        List<GameId>? initialGameIds = null,
        string? description = null,
        string? steamUrl = null,
        string? headerImage = null
        )
    {
        Name = name;
        SimplifiedName = SimplifyName(name);
        Description = description;
        Offers = initialOffers ?? new List<GameOffer>();
        GameIds = initialGameIds ?? new List<GameId>();
        SteamURL = steamUrl;
        HeaderImage = headerImage;
    }
    
    private static string SimplifyName(string name)
    {
        var words = name.Split(' ').ToList();
        if (words.Last().ToLower().Equals("key"))
        {
            words.RemoveAt(words.Count - 1);
            return string.Join(' ', words);
        }
        return name;
    }

    public class GameId
    {
        [BsonRepresentation(BsonType.String)]
        [BsonElement("vendor")]
        public EVendor Vendor { get; set; }
        [BsonElement("id")]
        public string Id { get; set; }

        public GameId(EVendor vendor, string id)
        {
            Vendor = vendor;
            Id = id;
        }
    }
}
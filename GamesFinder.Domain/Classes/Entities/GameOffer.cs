using GamesFinder.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace GamesFinder.Domain.Classes.Entities;

public class GameOffer : Entity
{
    [BsonElement("game_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid GameId { get; set; }
    [BsonElement("vendor")]
    public string Vendor { get; set; }
    [BsonElement("available")]
    public bool Available { get; set; }
    [BsonElement("price")]
    [BsonDictionaryOptions(DictionaryRepresentation.Document)]
    public Dictionary<ECurrency, (decimal, decimal)> Prices { get; set; }

    public GameOffer(Guid gameId, String vendor, Dictionary<ECurrency, (decimal, decimal)> prices, bool available = false)
    {
        GameId = gameId;
        Vendor = vendor;
        Available = available;
        Prices = prices;
    }
}
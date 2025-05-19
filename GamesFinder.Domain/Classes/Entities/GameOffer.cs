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
    public EVendor Vendor { get; set; }
    [BsonElement("vendors_url")]
    public string VendorsUrl { get; set; }
    [BsonElement("available")]
    public bool Available { get; set; }
    [BsonElement("price")]
    [BsonDictionaryOptions(DictionaryRepresentation.Document)]
    public Dictionary<ECurrency, PriceRange> Prices { get; set; }

    public GameOffer(Guid gameId, EVendor vendor, string vendorsUrl, Dictionary<ECurrency, PriceRange> prices, bool available = false)
    {
        GameId = gameId;
        Vendor = vendor;
        VendorsUrl = vendorsUrl;
        Available = available;
        Prices = prices;
    }

    public class PriceRange
    {
        [BsonElement("initial")]
        public decimal? Initial { get; set; }
        [BsonElement("current")]
        public decimal? Current { get; set; }

        public PriceRange(decimal? initial, decimal? current)
        {
            Initial = initial;
            Current = current;
        }
    }
}
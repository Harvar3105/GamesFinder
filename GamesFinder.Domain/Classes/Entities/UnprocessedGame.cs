using GamesFinder.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace GamesFinder.Domain.Classes.Entities;

public class UnprocessedGame: Entity
{
    [BsonElement("vendors_name")]
    public string VendorsName { get; set; }
    [BsonElement("steam_name")]
    public string SteamName { get; set; }
    
    [BsonElement("steam_id")]
    public int SteamId { get; set; }
    
    [BsonElement("vendors_url")]
    public string? VendorsUrl { get; set; }
    
    [BsonElement("vendor_id")]
    public string? VendorsId { get; set; }
    
    [BsonElement("currency")]
    public ECurrency? Currency { get; set; }
    
    [BsonElement("price")]
    public decimal? Price { get; set; }

    public UnprocessedGame(
        string vendorsName,
        int steamId,
        string steamName,
        string? vendorsUrl = null,
        string? vendorsId = null,
        ECurrency? currency = null,
        decimal? price = null
        )
    {
        VendorsName = vendorsName;
        SteamId = steamId;
        SteamName = steamName;
        VendorsUrl = vendorsUrl;
        VendorsId = vendorsId;
        Currency = currency;
        Price = price;
    }
}
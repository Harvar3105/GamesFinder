using GamesFinder.Domain.Enums;

namespace GamesFinder.Domain.Entities;

public class GameOffer : Entity
{
    public Guid GameId { get; }
    public Guid VendorId { get; }
    public bool Available { get; set; }
    public Dictionary<ECurrency, decimal?> Prices { get; }

    protected GameOffer(Guid gameId, Guid vendorId, Dictionary<ECurrency, decimal?> prices, bool available = false)
    {
        GameId = gameId;
        VendorId = vendorId;
        Available = available;
        Prices = prices;
    }
}
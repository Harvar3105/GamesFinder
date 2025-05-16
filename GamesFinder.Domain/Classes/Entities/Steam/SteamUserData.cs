using System.Text.Json.Serialization;

namespace GamesFinder.Domain.Classes.Entities.Steam;

public class SteamUserData
{
    [JsonPropertyName("rgWishlist")]
    public List<string> WishList { get; set; }
    [JsonPropertyName("rgOwnedPackages")]
    public List<string> OwnedPackages { get; set; }
    [JsonPropertyName("rgOwnedApps")]
    public List<string> OwnedApps { get; set; }
}
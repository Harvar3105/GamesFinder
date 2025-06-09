using GamesFinder.Domain.Enums;

namespace GamesFinder.Domain.Classes.Entities.DTOs;

public class GameDto : Entity
{
    public string Name { get; set; }
    public string SimplifiedName { get; set; }
    public string? SteamURL { get; set; }
    public List<Game.GameId> GameIds { get; set; } = new();
    public List<int> InPackages { get; set; } = new();
    public EType Type { get; set; }
    public string? Description { get; set; }
    public string? HeaderImage { get; set; }
    public List<GameOffer> Offers { get; set; } = new();
}

public static class GameExtensions
{
    public static GameDto ToDto(this Game game)
    {
        return new GameDto
        {
            Id = game.Id,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt,
            
            Name = game.Name,
            SimplifiedName = game.SimplifiedName,
            SteamURL = game.SteamURL,
            GameIds = game.GameIds,
            Description = game.Description,
            HeaderImage = game.HeaderImage,
            Offers = game.Offers
        };
    }
}
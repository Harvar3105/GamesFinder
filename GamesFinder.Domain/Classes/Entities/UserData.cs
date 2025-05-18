using GamesFinder.Domain.Interfaces.Crawlers;
using MongoDB.Bson.Serialization.Attributes;

namespace GamesFinder.Domain.Classes.Entities;

public class UserData : Entity
{
    [BsonElement("user_id")]
    public Guid UserId { get; set; }
    [BsonElement("users_wishlist")]
    public List<string> UsersWishlist { get; set; } = new();
    [BsonElement("avatar_file_name")]
    public string? AvatarFileName { get; set; }
    [BsonElement("avatar_content")]
    public byte[]? AvatarContent { get; set; }
    [BsonElement("avatar_file_type")]
    public string? AvatarFileType { get; set; }

    public UserData(Guid userId, string? avatarFileName, string? avatarContentType, byte[]? avatarContent)
    {
        UserId = userId;
        AvatarFileName = avatarFileName;
        AvatarContent = avatarContent;
        AvatarFileType = avatarContentType;
    }
}
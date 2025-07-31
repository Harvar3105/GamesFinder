using GamesFinder.Domain.Interfaces.Crawlers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GamesFinder.Domain.Classes.Entities;

public class UserData : Entity
{
    [BsonIgnore]
    public new Guid Id { get; set; }

    [BsonElement("_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }
    
    [BsonElement("users_wishlist")]
    public List<int> UsersWishlist { get; set; } = new();
    
    [BsonElement("avatar_file_name")]
    public string? AvatarFileName { get; set; }
    
    [BsonElement("avatar_content")]
    public byte[]? AvatarContent { get; set; }
    
    [BsonElement("avatar_file_type")]
    public string? AvatarFileType { get; set; }

    public UserData(Guid userId, string? avatarFileName, string? avatarContentType, byte[]? avatarContent, List<int> usersWishlist)
    {
        UserId = userId;
        AvatarFileName = avatarFileName;
        AvatarContent = avatarContent;
        AvatarFileType = avatarContentType;
        UsersWishlist = usersWishlist;
    }
}
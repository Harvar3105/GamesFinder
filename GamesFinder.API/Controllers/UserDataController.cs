using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesFinder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserDataController : ControllerBase
{
    private readonly ILogger<UserDataController> _logger;
    private readonly IUserDataRepository _userDataRepository;

    public UserDataController(ILogger<UserDataController> logger, IUserDataRepository userDataRepository)
    {
        _logger = logger;
        _userDataRepository = userDataRepository;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SaveUserData([FromBody] UserDataModel model)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            _logger.LogError("Could not find user id!");
            return BadRequest("Could not find user id!");
        }
        
        var userData = new UserData(
            userId: Guid.Parse(userId),
            avatarFileName: model.AvatarFileName,
            avatarContent: model.AvatarContent,
            avatarContentType: model.AvatarFileType,
            usersWishlist: model.UsersWishlist?.ToList() ?? new List<int>()
        );

        var success = await _userDataRepository.SaveOrUpdateAsync(userData);
        if (!success)
        {
            _logger.LogError("Could not save user data!");
            return BadRequest("Could not save user data!");
        }

        return Ok("Saved");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUserData()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            _logger.LogError("Could not find user id!");
            return BadRequest("Could not find user id!");
        }
        
        var result = await _userDataRepository.GetByIdAsync(Guid.Parse(userId));
        if (result == null)
        {
            _logger.LogError("Could not find user data!");
            return NotFound();
        }
        
        return Ok(result);
    }
}

public class UserDataModel
{
    public string? UserId { get; set; }
    public IEnumerable<int>? UsersWishlist { get; set; }
    public byte[]? AvatarContent { get; set; }
    public string? AvatarFileName { get; set; }
    public string? AvatarFileType { get; set; }
}
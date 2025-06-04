using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesFinder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly ILogger<GamesController> _logger;
    private readonly IGameRepository<Game> _gameRepository;

    public GamesController(ILogger<GamesController> logger, IGameRepository<Game> gameRepository)
    {
        _logger = logger;
        _gameRepository = gameRepository;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllGames()
    {
        _logger.LogInformation("Getting all games...");
        var games = await _gameRepository.GetAllAsync();
        
        return Ok(games);
    }
}
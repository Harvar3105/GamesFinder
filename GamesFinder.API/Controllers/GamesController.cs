using GamesFinder.Application.Services;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesFinder.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class GamesController : ControllerBase
{
    private readonly ILogger<GamesController> _logger;
    private readonly GamesWithOffersService _gamesWithOffersService;

    public GamesController(ILogger<GamesController> logger,GamesWithOffersService gamesWithOffersService)
    {
        _logger = logger;
        _gamesWithOffersService = gamesWithOffersService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllGames()
    {
        _logger.LogInformation("Getting all games...");
        var games = await _gamesWithOffersService.GetAllAsync();
        
        return Ok(games);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> GetPaged(int page, int pageSize)
    {
        var games = await _gamesWithOffersService.GetPagedAsync(page, pageSize);
        var totalCount = await _gamesWithOffersService.CountAsync();
        
        return Ok(new
        {
            Items = games,
            Count = games.Count(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _gamesWithOffersService.GetByIdAsync(id);
        return Ok(new
        {
            Result = result,
            Id = id
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> GetByAppId(int appId)
    {
        return Ok(await _gamesWithOffersService.GetByAppId(appId));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> GetByAppIds(IEnumerable<int> ids)
    {
        var result = await _gamesWithOffersService.GetByAppIds(ids);
        
        return Ok(new
        {
            Result = result,
            Amount = result?.Count(),
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Exists(Guid id)
    {
        var result = await _gamesWithOffersService.ExistsAsync(id);
        if (result) return Ok();
        else return NotFound();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> CountGames()
    {
        return Ok(new
        {
            Amount = await _gamesWithOffersService.CountAsync()
        });
    }
}
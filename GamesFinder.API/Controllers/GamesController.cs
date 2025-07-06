using GamesFinder.Application.Services;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Classes.Entities.DTOs;
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
    private readonly IUnprocessedGamesRepository<UnprocessedGame> _unprocessedGamesRepository;

    public GamesController(ILogger<GamesController> logger,GamesWithOffersService gamesWithOffersService, IUnprocessedGamesRepository<UnprocessedGame> unprocessedGamesRepository)
    {
        _logger = logger;
        _gamesWithOffersService = gamesWithOffersService;
        _unprocessedGamesRepository = unprocessedGamesRepository;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllGames()
    {
        _logger.LogInformation("Getting all games...");
        var games = await _gamesWithOffersService.GetAllAsync();
        
        return Ok(games);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetPaged(int page, int pageSize)
    {
        _logger.LogInformation($"Getting paged games with page {page} pageSize {pageSize}...");
        var games = (await _gamesWithOffersService.GetPagedAsync(page, pageSize)).Select(g => g.ToDto());
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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUnprocessedPagedGames(int page, int pageSize)
    {
        _logger.LogInformation($"Getting unprocessed paged games with page {page} pageSize {pageSize}...");
        var games = await _unprocessedGamesRepository.GetPagedAsync(page, pageSize);
        var totalCount = await _unprocessedGamesRepository.CountAsync();
        
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
    public async Task<IActionResult> CheckExistManyBySteamIds([FromBody] List<string> appIds)
    {
        _logger.LogInformation("Checking existing games...");
        var unexistingGamesIds = await _gamesWithOffersService.CheckExistManyBySteamIds(appIds);
        _logger.LogInformation($"Unexisting games with ids {unexistingGamesIds}");
        return Ok(unexistingGamesIds);
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
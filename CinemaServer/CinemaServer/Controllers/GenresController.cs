using Microsoft.AspNetCore.Mvc;
using CinemaServer.DTOs;
using CinemaServer.Services;

namespace CinemaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GenresController : ControllerBase
{
    private readonly GenreService _genreService;

    public GenresController(GenreService genreService)
    {
        _genreService = genreService;
    }

    /// <summary>
    /// Получение всех жанров
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<GenreResponse>>), 200)]
    public async Task<ActionResult<ApiResponse<List<GenreResponse>>>> GetAll()
    {
        var genres = await _genreService.GetAllAsync();
        return Ok(new ApiResponse<List<GenreResponse>>
        {
            Success = true,
            Data = genres
        });
    }
}

using Microsoft.AspNetCore.Mvc;
using CinemaServer.DTOs;
using CinemaServer.Services;

namespace CinemaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CollectionsController : ControllerBase
{
    private readonly CollectionService _collectionService;

    public CollectionsController(CollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    /// <summary>
    /// Получение избранных коллекций (для главной страницы)
    /// </summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(ApiResponse<List<CollectionResponse>>), 200)]
    public async Task<ActionResult<ApiResponse<List<CollectionResponse>>>> GetFeatured()
    {
        var collections = await _collectionService.GetFeaturedAsync();
        return Ok(new ApiResponse<List<CollectionResponse>>
        {
            Success = true,
            Data = collections
        });
    }
}

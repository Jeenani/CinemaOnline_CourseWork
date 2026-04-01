using Microsoft.AspNetCore.Mvc;
using CinemaServer.DTOs;
using CinemaServer.Services;

namespace CinemaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MoviesController : ControllerBase
{
    private readonly MovieService _movieService;
    private readonly RatingService _ratingService;
    private readonly CommentService _commentService;
    private readonly FavoriteService _favoriteService;
    private readonly ViewHistoryService _viewHistoryService;
    private readonly UserService _userService;

    public MoviesController(
        MovieService movieService,
        RatingService ratingService,
        CommentService commentService,
        FavoriteService favoriteService,
        ViewHistoryService viewHistoryService,
        UserService userService)
    {
        _movieService = movieService;
        _ratingService = ratingService;
        _commentService = commentService;
        _favoriteService = favoriteService;
        _viewHistoryService = viewHistoryService;
        _userService = userService;
    }

    /// <summary>
    /// Поиск фильмов с фильтрами
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<MovieResponse>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<MovieResponse>>>> Search([FromQuery] MovieSearchRequest request)
    {
        var result = await _movieService.SearchAsync(request);
        return Ok(new ApiResponse<PagedResponse<MovieResponse>>
        {
            Success = true,
            Data = result
        });
    }

    /// <summary>
    /// Получение случайного фильма для баннера
    /// </summary>
    [HttpGet("random")]
    [ProducesResponseType(typeof(ApiResponse<MovieResponse>), 200)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> GetRandom()
    {
        var movie = await _movieService.GetRandomAsync();
        return Ok(new ApiResponse<MovieResponse>
        {
            Success = true,
            Data = movie
        });
    }

    /// <summary>
    /// Получение списка стран
    /// </summary>
    [HttpGet("countries")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetCountries()
    {
        var countries = await _movieService.GetCountriesAsync();
        return Ok(new ApiResponse<List<string>>
        {
            Success = true,
            Data = countries
        });
    }

    /// <summary>
    /// Получение фильма по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<MovieDetailResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<ActionResult<ApiResponse<MovieDetailResponse>>> GetById(
        long id,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        var movie = await _movieService.GetByIdAsync(id, userId);

        if (movie == null)
        {
            return NotFound(new ErrorResponse { Message = "Фильм не найден" });
        }

        return Ok(new ApiResponse<MovieDetailResponse>
        {
            Success = true,
            Data = movie
        });
    }

    /// <summary>
    /// Получение видео URL (проверка доступа к подписке)
    /// </summary>
    [HttpGet("{id}/video")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    public async Task<ActionResult> GetVideo(
        long id,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var movie = await _movieService.GetByIdAsync(id);
        if (movie == null)
        {
            return NotFound(new ErrorResponse { Message = "Фильм не найден" });
        }

        // Если фильм требует подписку
        if (movie.NeedSubscription)
        {
            var userId = AuthController.GetUserIdFromToken(authorization);
            if (userId == null)
            {
                return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
            }

            var hasPremium = await _userService.HasPremiumSubscriptionAsync(userId.Value);
            if (!hasPremium)
            {
                return StatusCode(403, new ErrorResponse { Message = "Требуется подписка Premium" });
            }
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = new { videoUrl = movie.VideoUrl }
        });
    }

    /// <summary>
    /// Создание фильма (только админ)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<long>), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    public async Task<ActionResult<ApiResponse<long>>> Create(
        [FromBody] CreateMovieRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization))
        {
            return StatusCode(403, new ErrorResponse { Message = "Доступ запрещен" });
        }

        var movieId = await _movieService.CreateAsync(request);
        
        return CreatedAtAction(nameof(GetById), new { id = movieId }, new ApiResponse<long>
        {
            Success = true,
            Data = movieId,
            Message = "Фильм создан"
        });
    }

    /// <summary>
    /// Обновление фильма (только админ)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    public async Task<ActionResult<ApiResponse<bool>>> Update(
        long id,
        [FromBody] UpdateMovieRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization))
        {
            return StatusCode(403, new ErrorResponse { Message = "Доступ запрещен" });
        }

        var success = await _movieService.UpdateAsync(id, request);
        return Ok(new ApiResponse<bool>
        {
            Success = success,
            Data = success,
            Message = success ? "Фильм обновлен" : "Фильм не найден"
        });
    }

    /// <summary>
    /// Удаление фильма (только админ)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(
        long id,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization))
        {
            return StatusCode(403, new ErrorResponse { Message = "Доступ запрещен" });
        }

        var success = await _movieService.DeleteAsync(id);
        return Ok(new ApiResponse<bool>
        {
            Success = success,
            Data = success,
            Message = success ? "Фильм удален" : "Фильм не найден"
        });
    }

    /// <summary>
    /// Оценка фильма
    /// </summary>
    [HttpPost("{id}/rate")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<bool>>> Rate(
        long id,
        [FromBody] RateMovieRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
        }

        // Проверка подписки для премиум-фильмов
        var movie = await _movieService.GetByIdAsync(id);
        if (movie != null && movie.NeedSubscription)
        {
            var hasPremium = await _userService.HasPremiumSubscriptionAsync(userId.Value);
            if (!hasPremium)
            {
                return StatusCode(403, new ErrorResponse { Message = "Для оценки этого фильма требуется подписка Premium" });
            }
        }

        var success = await _ratingService.RateMovieAsync(userId.Value, id, request.Rating);
        return Ok(new ApiResponse<bool>
        {
            Success = success,
            Data = success,
            Message = "Оценка сохранена"
        });
    }

    /// <summary>
    /// Получение комментариев к фильму
    /// </summary>
    [HttpGet("{id}/comments")]
    [ProducesResponseType(typeof(ApiResponse<List<CommentResponse>>), 200)]
    public async Task<ActionResult<ApiResponse<List<CommentResponse>>>> GetComments(long id)
    {
        var comments = await _commentService.GetByMovieIdAsync(id);
        return Ok(new ApiResponse<List<CommentResponse>>
        {
            Success = true,
            Data = comments
        });
    }

    /// <summary>
    /// Добавление комментария
    /// </summary>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(typeof(ApiResponse<long>), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<long>>> AddComment(
        long id,
        [FromBody] CreateCommentRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
        }

        var commentId = await _commentService.CreateAsync(userId.Value, id, request.Content);
        return Created($"/api/movies/{id}/comments", new ApiResponse<long>
        {
            Success = true,
            Data = commentId,
            Message = "Комментарий добавлен"
        });
    }

    /// <summary>
    /// Удаление комментария
    /// </summary>
    [HttpDelete("comments/{commentId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteComment(
        long commentId,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
        }

        var isAdmin = IsAdmin(authorization);
        var success = await _commentService.DeleteAsync(commentId, userId.Value, isAdmin);
        
        return Ok(new ApiResponse<bool>
        {
            Success = success,
            Data = success,
            Message = success ? "Комментарий удален" : "Комментарий не найден или нет прав"
        });
    }

    /// <summary>
    /// Добавление в избранное
    /// </summary>
    [HttpPost("{id}/favorite")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<bool>>> AddToFavorites(
        long id,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
        }

        var success = await _favoriteService.AddAsync(userId.Value, id);
        return Ok(new ApiResponse<bool>
        {
            Success = success,
            Data = success,
            Message = "Добавлено в избранное"
        });
    }

    /// <summary>
    /// Удаление из избранного
    /// </summary>
    [HttpDelete("{id}/favorite")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFromFavorites(
        long id,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
        }

        var success = await _favoriteService.RemoveAsync(userId.Value, id);
        return Ok(new ApiResponse<bool>
        {
            Success = success,
            Data = success,
            Message = "Удалено из избранного"
        });
    }

    /// <summary>
    /// Запись прогресса просмотра
    /// </summary>
    [HttpPost("{id}/view")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<bool>>> RecordView(
        long id,
        [FromBody] RecordViewRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
        }

        var success = await _viewHistoryService.RecordAsync(userId.Value, id, request.ProgressSeconds, request.Completed);
        return Ok(new ApiResponse<bool>
        {
            Success = success,
            Data = success
        });
    }

    private static bool IsAdmin(string? authorization)
    {
        var role = AuthController.GetUserRoleFromToken(authorization);
        return role == "admin";
    }
}

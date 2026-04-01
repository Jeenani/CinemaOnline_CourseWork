using Microsoft.AspNetCore.Mvc;
using CinemaServer.DTOs;
using CinemaServer.Services;

namespace CinemaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly FavoriteService _favoriteService;
    private readonly ViewHistoryService _viewHistoryService;
    private readonly UserService _userService;

    public UserController(
        FavoriteService favoriteService,
        ViewHistoryService viewHistoryService,
        UserService userService)
    {
        _favoriteService = favoriteService;
        _viewHistoryService = viewHistoryService;
        _userService = userService;
    }

    /// <summary>
    /// Обновление профиля пользователя (имя, email)
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new ErrorResponse { Message = "Имя и email обязательны" });

        var success = await _userService.UpdateProfileAsync(userId.Value, request.Name.Trim(), request.Email.Trim());
        if (!success)
            return BadRequest(new ErrorResponse { Message = "Email уже используется другим пользователем" });

        return Ok(new ApiResponse<bool> { Success = true, Data = true, Message = "Профиль обновлён" });
    }

    /// <summary>
    /// Смена пароля
    /// </summary>
    [HttpPut("password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return BadRequest(new ErrorResponse { Message = "Новый пароль должен быть не менее 6 символов" });

        // Получаем текущий хеш пароля для проверки
        var user = await _userService.GetByIdAsync(userId.Value);
        if (user == null)
            return Unauthorized(new ErrorResponse { Message = "Пользователь не найден" });

        var currentHash = await _userService.GetPasswordHashByIdAsync(userId.Value);
        if (currentHash == null || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, currentHash))
            return BadRequest(new ErrorResponse { Message = "Неверный текущий пароль" });

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        var success = await _userService.ChangePasswordAsync(userId.Value, newHash);

        return Ok(new ApiResponse<bool> { Success = success, Data = success, Message = "Пароль изменён" });
    }

    /// <summary>
    /// Получение информации о подписке пользователя
    /// </summary>
    [HttpGet("subscription")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionInfo>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<SubscriptionInfo>>> GetSubscription(
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
        }

        var user = await _userService.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Пользователь не найден" });
        }

        return Ok(new ApiResponse<SubscriptionInfo>
        {
            Success = true,
            Data = user.Subscription
        });
    }

    /// <summary>
    /// Получение избранных фильмов
    /// </summary>
    [HttpGet("favorites")]
    [ProducesResponseType(typeof(ApiResponse<List<MovieResponse>>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<List<MovieResponse>>>> GetFavorites(
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
        }

        var favorites = await _favoriteService.GetUserFavoritesAsync(userId.Value);
        return Ok(new ApiResponse<List<MovieResponse>>
        {
            Success = true,
            Data = favorites
        });
    }

    /// <summary>
    /// Получение истории просмотров
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<List<MovieResponse>>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<List<MovieResponse>>>> GetHistory(
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
        }

        var history = await _viewHistoryService.GetUserHistoryAsync(userId.Value);
        return Ok(new ApiResponse<List<MovieResponse>>
        {
            Success = true,
            Data = history
        });
    }
}

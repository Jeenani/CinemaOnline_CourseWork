using Microsoft.AspNetCore.Mvc;
using CinemaServer.DTOs;
using CinemaServer.Services;

namespace CinemaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var existing = await _userService.GetByEmailAsync(request.Email);
        if (existing != null)
        {
            return BadRequest(new ErrorResponse { Message = "Email уже зарегистрирован" });
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var userId = await _userService.CreateAsync(request.Email, passwordHash, request.Name);

        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            return BadRequest(new ErrorResponse { Message = "Ошибка создания пользователя" });
        }

        var token = GenerateToken(user);

        return Ok(new ApiResponse<AuthResponse>
        {
            Success = true,
            Data = new AuthResponse { Token = token, User = user }
        });
    }

    /// <summary>
    /// Авторизация пользователя
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Неверные учетные данные" });
        }

        var passwordHash = await _userService.GetPasswordHashAsync(request.Email);
        if (passwordHash == null || !BCrypt.Net.BCrypt.Verify(request.Password, passwordHash))
        {
            return Unauthorized(new ErrorResponse { Message = "Неверные учетные данные" });
        }

        var token = GenerateToken(user);

        return Ok(new ApiResponse<AuthResponse>
        {
            Success = true,
            Data = new AuthResponse { Token = token, User = user }
        });
    }

    /// <summary>
    /// Получение текущего пользователя по токену
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetMe([FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = GetUserIdFromToken(authorization);
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
        }

        var user = await _userService.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Пользователь не найден" });
        }

        return Ok(new ApiResponse<UserResponse>
        {
            Success = true,
            Data = user
        });
    }

    // Простая генерация токена (для продакшена использовать JWT)
    private static string GenerateToken(UserResponse user)
    {
        return $"Bearer_{user.Id}_{user.Role}_{Guid.NewGuid():N}";
    }

    // Парсинг userId из токена
    internal static long? GetUserIdFromToken(string? authorization)
    {
        if (string.IsNullOrEmpty(authorization)) return null;
        
        var token = authorization.Replace("Bearer ", "");
        var parts = token.Split('_');
        
        if (parts.Length >= 2 && long.TryParse(parts[1], out var userId))
        {
            return userId;
        }
        return null;
    }

    internal static string? GetUserRoleFromToken(string? authorization)
    {
        if (string.IsNullOrEmpty(authorization)) return null;
        
        var token = authorization.Replace("Bearer ", "");
        var parts = token.Split('_');
        
        return parts.Length >= 3 ? parts[2] : null;
    }
}

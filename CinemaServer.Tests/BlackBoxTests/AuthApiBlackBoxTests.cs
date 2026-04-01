using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.BlackBoxTests;

/// <summary>
/// Тесты чёрного ящика для API авторизации.
/// Тестируется поведение контроллера без знания внутренней реализации.
/// Проверяются HTTP-коды ответов, структура данных, бизнес-логика с точки зрения пользователя.
/// </summary>
public class AuthApiBlackBoxTests
{
    private AuthController CreateController()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        return new AuthController(userService);
    }

    // =============================================
    // Регистрация — тесты чёрного ящика
    // =============================================

    /// <summary>
    /// Успешная регистрация нового пользователя → 200 OK, токен и данные пользователя
    /// </summary>
    [Fact]
    public async Task Register_ValidNewUser_ShouldReturn200WithTokenAndUser()
    {
        var controller = CreateController();
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "securePass123",
            Name = "Новый Пользователь"
        };

        var result = await controller.Register(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Token.Should().NotBeNullOrEmpty();
        response.Data.User.Should().NotBeNull();
        response.Data.User.Email.Should().Be("newuser@example.com");
        response.Data.User.Name.Should().Be("Новый Пользователь");
    }

    /// <summary>
    /// Регистрация с уже существующим email → 400 Bad Request
    /// </summary>
    [Fact]
    public async Task Register_ExistingEmail_ShouldReturn400()
    {
        var controller = CreateController();
        var request = new RegisterRequest
        {
            Email = "user@test.com", // Уже существует
            Password = "password123",
            Name = "Дубль"
        };

        var result = await controller.Register(request);

        var badResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badResult.StatusCode.Should().Be(400);
    }

    // =============================================
    // Авторизация — тесты чёрного ящика
    // =============================================

    /// <summary>
    /// Успешная авторизация → 200 OK с токеном
    /// </summary>
    [Fact]
    public async Task Login_ValidCredentials_ShouldReturn200WithToken()
    {
        var controller = CreateController();
        var request = new LoginRequest
        {
            Email = "user@test.com",
            Password = "password123"
        };

        var result = await controller.Login(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Token.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Авторизация с неверным паролем → 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task Login_WrongPassword_ShouldReturn401()
    {
        var controller = CreateController();
        var request = new LoginRequest
        {
            Email = "user@test.com",
            Password = "wrongpassword"
        };

        var result = await controller.Login(request);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Авторизация с несуществующим email → 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task Login_NonExistingEmail_ShouldReturn401()
    {
        var controller = CreateController();
        var request = new LoginRequest
        {
            Email = "nonexistent@test.com",
            Password = "password123"
        };

        var result = await controller.Login(request);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // =============================================
    // GetMe — тесты чёрного ящика
    // =============================================

    /// <summary>
    /// Получение профиля с валидным токеном → 200 OK
    /// </summary>
    [Fact]
    public async Task GetMe_ValidToken_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.GetMe("Bearer_1_user_someguid");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<UserResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Email.Should().Be("user@test.com");
    }

    /// <summary>
    /// Получение профиля без токена → 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task GetMe_NoToken_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.GetMe(null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Получение профиля с невалидным токеном → 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task GetMe_InvalidToken_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.GetMe("Bearer_999_user_guid");

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}

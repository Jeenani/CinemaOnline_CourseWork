using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.BlackBoxTests;

/// <summary>
/// Тесты чёрного ящика для API пользователя (UserController).
/// Покрываются сценарии: профиль, смена пароля, подписка, избранное, история.
/// </summary>
public class UserApiBlackBoxTests
{
    private readonly string _userToken = "Bearer_1_user_guid";
    private readonly string _premiumToken = "Bearer_2_user_guid";

    private (UserController controller, AuthController authController) CreateControllers()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var favoriteService = new FavoriteService(context);
        var viewHistoryService = new ViewHistoryService(context);
        var userController = new UserController(favoriteService, viewHistoryService, userService);
        var authController = new AuthController(userService);
        return (userController, authController);
    }

    // =============================================
    // Обновление профиля — положительные сценарии
    // =============================================

    /// <summary>
    /// Обновление профиля с корректными данными → 200 OK
    /// </summary>
    [Fact]
    public async Task UpdateProfile_ValidData_ShouldReturn200()
    {
        var (controller, _) = CreateControllers();
        var request = new UpdateProfileRequest
        {
            Name = "Обновлённое Имя",
            Email = "updated@test.com"
        };

        var result = await controller.UpdateProfile(request, _userToken);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        response.Success.Should().BeTrue();
    }

    // =============================================
    // Обновление профиля — негативные сценарии
    // =============================================

    /// <summary>
    /// Обновление профиля без авторизации → 401
    /// </summary>
    [Fact]
    public async Task UpdateProfile_NoAuth_ShouldReturn401()
    {
        var (controller, _) = CreateControllers();
        var request = new UpdateProfileRequest { Name = "Test", Email = "test@test.com" };

        var result = await controller.UpdateProfile(request, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Обновление профиля с пустым именем → 400
    /// </summary>
    [Fact]
    public async Task UpdateProfile_EmptyName_ShouldReturn400()
    {
        var (controller, _) = CreateControllers();
        var request = new UpdateProfileRequest { Name = "", Email = "test@test.com" };

        var result = await controller.UpdateProfile(request, _userToken);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Обновление профиля с пустым email → 400
    /// </summary>
    [Fact]
    public async Task UpdateProfile_EmptyEmail_ShouldReturn400()
    {
        var (controller, _) = CreateControllers();
        var request = new UpdateProfileRequest { Name = "Имя", Email = "" };

        var result = await controller.UpdateProfile(request, _userToken);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Обновление профиля с email, занятым другим пользователем → 400
    /// </summary>
    [Fact]
    public async Task UpdateProfile_DuplicateEmail_ShouldReturn400()
    {
        var (controller, _) = CreateControllers();
        var request = new UpdateProfileRequest
        {
            Name = "Имя",
            Email = "premium@test.com" // Уже используется пользователем 2
        };

        var result = await controller.UpdateProfile(request, _userToken);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =============================================
    // Смена пароля — положительные сценарии
    // =============================================

    /// <summary>
    /// Смена пароля с корректными данными → 200 OK
    /// Затем проверяем авторизацию с новым паролем
    /// </summary>
    [Fact]
    public async Task ChangePassword_ValidData_ShouldReturn200()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), userService);
        var authController = new AuthController(userService);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "password123",
            NewPassword = "newPassword456"
        };

        var result = await controller.ChangePassword(request, _userToken);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        response.Success.Should().BeTrue();

        // Проверяем что старый пароль больше не работает
        var loginOld = await authController.Login(new LoginRequest
        {
            Email = "user@test.com",
            Password = "password123"
        });
        loginOld.Result.Should().BeOfType<UnauthorizedObjectResult>();

        // Проверяем что новый пароль работает
        var loginNew = await authController.Login(new LoginRequest
        {
            Email = "user@test.com",
            Password = "newPassword456"
        });
        loginNew.Result.Should().BeOfType<OkObjectResult>();
    }

    // =============================================
    // Смена пароля — негативные сценарии
    // =============================================

    /// <summary>
    /// Смена пароля без авторизации → 401
    /// </summary>
    [Fact]
    public async Task ChangePassword_NoAuth_ShouldReturn401()
    {
        var (controller, _) = CreateControllers();
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "password123",
            NewPassword = "newPass123"
        };

        var result = await controller.ChangePassword(request, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Смена пароля с неверным текущим паролем → 400
    /// </summary>
    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ShouldReturn400()
    {
        var (controller, _) = CreateControllers();
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "wrongPassword",
            NewPassword = "newPass123"
        };

        var result = await controller.ChangePassword(request, _userToken);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Смена пароля с коротким новым паролем → 400
    /// </summary>
    [Fact]
    public async Task ChangePassword_ShortNewPassword_ShouldReturn400()
    {
        var (controller, _) = CreateControllers();
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "password123",
            NewPassword = "12345" // Менее 6 символов
        };

        var result = await controller.ChangePassword(request, _userToken);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Смена пароля с пустым новым паролем → 400
    /// </summary>
    [Fact]
    public async Task ChangePassword_EmptyNewPassword_ShouldReturn400()
    {
        var (controller, _) = CreateControllers();
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "password123",
            NewPassword = ""
        };

        var result = await controller.ChangePassword(request, _userToken);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =============================================
    // Подписка — положительные сценарии
    // =============================================

    /// <summary>
    /// Получение информации о подписке премиум-пользователя → 200 OK с данными подписки
    /// </summary>
    [Fact]
    public async Task GetSubscription_PremiumUser_ShouldReturnSubscriptionInfo()
    {
        var (controller, _) = CreateControllers();

        var result = await controller.GetSubscription(_premiumToken);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<SubscriptionInfo>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Name.Should().Be("Месячная");
    }

    /// <summary>
    /// Получение информации о подписке обычного пользователя → 200 OK, подписка null
    /// </summary>
    [Fact]
    public async Task GetSubscription_RegularUser_ShouldReturnNullSubscription()
    {
        var (controller, _) = CreateControllers();

        var result = await controller.GetSubscription(_userToken);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<SubscriptionInfo>>().Subject;
        response.Success.Should().BeTrue();
    }

    /// <summary>
    /// Получение подписки без авторизации → 401
    /// </summary>
    [Fact]
    public async Task GetSubscription_NoAuth_ShouldReturn401()
    {
        var (controller, _) = CreateControllers();

        var result = await controller.GetSubscription(null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // =============================================
    // Избранное — положительные сценарии
    // =============================================

    /// <summary>
    /// Получение избранного авторизованным пользователем → 200 OK со списком фильмов
    /// </summary>
    [Fact]
    public async Task GetFavorites_AuthUser_ShouldReturnFavorites()
    {
        var (controller, _) = CreateControllers();

        var result = await controller.GetFavorites(_userToken);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<List<MovieResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data.Should().HaveCountGreaterOrEqualTo(1); // В seed есть 1 избранный
    }

    /// <summary>
    /// Получение избранного без авторизации → 401
    /// </summary>
    [Fact]
    public async Task GetFavorites_NoAuth_ShouldReturn401()
    {
        var (controller, _) = CreateControllers();

        var result = await controller.GetFavorites(null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // =============================================
    // История просмотров — положительные сценарии
    // =============================================

    /// <summary>
    /// Получение истории просмотров авторизованным пользователем → 200 OK
    /// </summary>
    [Fact]
    public async Task GetHistory_AuthUser_ShouldReturnHistory()
    {
        var (controller, _) = CreateControllers();

        var result = await controller.GetHistory(_userToken);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<List<MovieResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data.Should().HaveCountGreaterOrEqualTo(1); // В seed есть 1 запись
    }

    /// <summary>
    /// Получение истории просмотров без авторизации → 401
    /// </summary>
    [Fact]
    public async Task GetHistory_NoAuth_ShouldReturn401()
    {
        var (controller, _) = CreateControllers();

        var result = await controller.GetHistory(null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Получение истории пользователя без просмотров → 200 OK, пустой список
    /// </summary>
    [Fact]
    public async Task GetHistory_UserWithNoHistory_ShouldReturnEmptyList()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), userService);

        // Пользователь 3 (admin) не имеет истории просмотров в seed
        var result = await controller.GetHistory("Bearer_3_admin_guid");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<List<MovieResponse>>>().Subject;
        response.Data.Should().BeEmpty();
    }
}

using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.IntegrationTests;

/// <summary>
/// Интеграционные тесты для профиля пользователя.
/// Проверяется взаимодействие: регистрация → изменение профиля → смена пароля → повторная авторизация.
/// </summary>
public class UserProfileIntegrationTests
{
    /// <summary>
    /// Полный цикл: регистрация → обновление профиля → проверка данных
    /// </summary>
    [Fact]
    public async Task Register_UpdateProfile_ShouldReflectChanges()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var authController = new AuthController(userService);
        var userController = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), userService);

        // 1. Регистрация
        var registerResult = await authController.Register(new RegisterRequest
        {
            Email = "profile@test.com",
            Password = "pass123456",
            Name = "Изначальное Имя"
        });
        var authData = (registerResult.Result as OkObjectResult)!.Value as ApiResponse<AuthResponse>;
        var token = authData!.Data!.Token;

        // 2. Обновление профиля
        var updateResult = await userController.UpdateProfile(
            new UpdateProfileRequest { Name = "Обновлённое Имя", Email = "profile_updated@test.com" },
            token);
        updateResult.Result.Should().BeOfType<OkObjectResult>();

        // 3. Проверяем через GetMe, что данные обновлены
        var meResult = await authController.GetMe(token);
        var meData = (meResult.Result as OkObjectResult)!.Value as ApiResponse<UserResponse>;
        meData!.Data!.Name.Should().Be("Обновлённое Имя");
        meData.Data.Email.Should().Be("profile_updated@test.com");
    }

    /// <summary>
    /// Полный цикл: регистрация → смена пароля → авторизация с новым паролем
    /// </summary>
    [Fact]
    public async Task Register_ChangePassword_LoginWithNewPassword()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var authController = new AuthController(userService);
        var userController = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), userService);

        // 1. Регистрация
        var registerResult = await authController.Register(new RegisterRequest
        {
            Email = "pwdchange@test.com",
            Password = "originalPass123",
            Name = "Пользователь"
        });
        var authData = (registerResult.Result as OkObjectResult)!.Value as ApiResponse<AuthResponse>;
        var token = authData!.Data!.Token;

        // 2. Смена пароля
        var changeResult = await userController.ChangePassword(
            new ChangePasswordRequest { CurrentPassword = "originalPass123", NewPassword = "newSecurePass456" },
            token);
        changeResult.Result.Should().BeOfType<OkObjectResult>();

        // 3. Старый пароль НЕ работает
        var loginOld = await authController.Login(new LoginRequest
        {
            Email = "pwdchange@test.com",
            Password = "originalPass123"
        });
        loginOld.Result.Should().BeOfType<UnauthorizedObjectResult>();

        // 4. Новый пароль РАБОТАЕТ
        var loginNew = await authController.Login(new LoginRequest
        {
            Email = "pwdchange@test.com",
            Password = "newSecurePass456"
        });
        loginNew.Result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Цикл: регистрация → покупка подписки → проверка подписки в профиле
    /// </summary>
    [Fact]
    public async Task Register_BuySubscription_CheckProfileSubscription()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var paymentService = new PaymentService(context, userService, subService);
        var authController = new AuthController(userService);
        var payController = new PaymentsController(paymentService);
        var userController = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), userService);

        // 1. Регистрация
        var registerResult = await authController.Register(new RegisterRequest
        {
            Email = "subscriber@test.com",
            Password = "pass123456",
            Name = "Подписчик"
        });
        var authData = (registerResult.Result as OkObjectResult)!.Value as ApiResponse<AuthResponse>;
        var token = authData!.Data!.Token;

        // 2. Проверяем — подписки нет
        var subBefore = await userController.GetSubscription(token);
        var subBeforeData = (subBefore.Result as OkObjectResult)!.Value as ApiResponse<SubscriptionInfo>;
        subBeforeData!.Data.Should().BeNull();

        // 3. Создаём платёж
        var payResult = await payController.Create(
            new CreatePaymentRequest { SubscriptionId = 1, PaymentMethod = "card" }, token);
        var payData = (payResult.Result as CreatedResult)!.Value as ApiResponse<PaymentResponse>;

        // 4. Обрабатываем платёж (успех)
        await payController.Process(payData!.Data!.Id, new ProcessPaymentRequest { Success = true });

        // 5. Проверяем — подписка появилась
        var subAfter = await userController.GetSubscription(token);
        var subAfterData = (subAfter.Result as OkObjectResult)!.Value as ApiResponse<SubscriptionInfo>;
        subAfterData!.Data.Should().NotBeNull();
        subAfterData.Data!.Name.Should().Be("Месячная");
    }

    /// <summary>
    /// Цикл: добавление в избранное → проверка в списке → удаление → проверка удаления
    /// </summary>
    [Fact]
    public async Task AddFavorite_CheckList_RemoveFavorite_CheckEmpty()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var favoriteService = new FavoriteService(context);
        var moviesController = new MoviesController(
            new MovieService(context), new RatingService(context),
            new CommentService(context), favoriteService,
            new ViewHistoryService(context), userService);
        var userController = new UserController(favoriteService, new ViewHistoryService(context), userService);

        var token = "Bearer_2_user_guid"; // premium user, без избранного в seed

        // 1. Добавляем фильм 1 в избранное
        var addResult = await moviesController.AddToFavorites(1, token);
        addResult.Result.Should().BeOfType<OkObjectResult>();

        // 2. Проверяем список избранного
        var favs = await userController.GetFavorites(token);
        var favsData = (favs.Result as OkObjectResult)!.Value as ApiResponse<List<MovieResponse>>;
        favsData!.Data.Should().Contain(m => m.Id == 1);

        // 3. Удаляем из избранного
        var removeResult = await moviesController.RemoveFromFavorites(1, token);
        removeResult.Result.Should().BeOfType<OkObjectResult>();

        // 4. Проверяем, что список пуст
        var favsAfter = await userController.GetFavorites(token);
        var favsAfterData = (favsAfter.Result as OkObjectResult)!.Value as ApiResponse<List<MovieResponse>>;
        favsAfterData!.Data.Should().NotContain(m => m.Id == 1);
    }

    /// <summary>
    /// Цикл: запись просмотра → проверка в истории
    /// </summary>
    [Fact]
    public async Task RecordView_CheckHistory()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var viewHistoryService = new ViewHistoryService(context);
        var moviesController = new MoviesController(
            new MovieService(context), new RatingService(context),
            new CommentService(context), new FavoriteService(context),
            viewHistoryService, userService);
        var userController = new UserController(
            new FavoriteService(context), viewHistoryService, userService);

        var token = "Bearer_2_user_guid";

        // 1. Записываем просмотр фильма 1
        var viewResult = await moviesController.RecordView(1,
            new RecordViewRequest { ProgressSeconds = 7200, Completed = true }, token);
        viewResult.Result.Should().BeOfType<OkObjectResult>();

        // 2. Проверяем историю
        var history = await userController.GetHistory(token);
        var histData = (history.Result as OkObjectResult)!.Value as ApiResponse<List<MovieResponse>>;
        histData!.Data.Should().Contain(m => m.Id == 1);
    }
}

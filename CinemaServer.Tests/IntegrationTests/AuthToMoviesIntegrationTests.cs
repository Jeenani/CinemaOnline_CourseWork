using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.IntegrationTests;

/// <summary>
/// Интеграционные тесты: взаимодействие модулей авторизации и фильмов.
/// Проверяется, что AuthController корректно работает с MoviesController,
/// токены парсятся, права доступа проверяются.
/// </summary>
public class AuthToMoviesIntegrationTests
{
    /// <summary>
    /// Сценарий: пользователь регистрируется → получает токен → использует токен для оценки фильма
    /// </summary>
    [Fact]
    public async Task RegisterThenRateMovie_ShouldWorkEndToEnd()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var authController = new AuthController(userService);
        var moviesController = new MoviesController(
            new MovieService(context),
            new RatingService(context),
            new CommentService(context),
            new FavoriteService(context),
            new ViewHistoryService(context),
            userService
        );

        // 1. Регистрация
        var registerResult = await authController.Register(new RegisterRequest
        {
            Email = "integration@test.com",
            Password = "integrationPass",
            Name = "Integration User"
        });
        var okRegister = registerResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var authResponse = okRegister.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        var token = authResponse.Data!.Token;

        // 2. Используем токен для оценки фильма
        var rateResult = await moviesController.Rate(1, new RateMovieRequest { Rating = 5 }, token);
        var okRate = rateResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var rateResponse = okRate.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        rateResponse.Success.Should().BeTrue();
    }

    /// <summary>
    /// Сценарий: пользователь логинится → добавляет комментарий → получает фильм с комментарием
    /// </summary>
    [Fact]
    public async Task LoginThenCommentThenGetMovie_ShouldShowComment()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var authController = new AuthController(userService);
        var moviesController = new MoviesController(
            new MovieService(context),
            new RatingService(context),
            new CommentService(context),
            new FavoriteService(context),
            new ViewHistoryService(context),
            userService
        );

        // 1. Логин
        var loginResult = await authController.Login(new LoginRequest
        {
            Email = "user@test.com",
            Password = "password123"
        });
        var authResponse = (loginResult.Result as OkObjectResult)!.Value as ApiResponse<AuthResponse>;
        var token = authResponse!.Data!.Token;

        // 2. Добавляем комментарий
        await moviesController.AddComment(1, new CreateCommentRequest { Content = "Интеграционный тест" }, token);

        // 3. Получаем фильм — комментарий должен быть
        var movieResult = await moviesController.GetById(1, token);
        var movieOk = movieResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var movieResponse = movieOk.Value.Should().BeOfType<ApiResponse<MovieDetailResponse>>().Subject;
        movieResponse.Data!.Comments.Should().Contain(c => c.Content == "Интеграционный тест");
    }

    /// <summary>
    /// Сценарий: пользователь логинится → добавляет в избранное → получает список избранного
    /// </summary>
    [Fact]
    public async Task LoginThenFavoriteThenGetFavorites_ShouldIncludeMovie()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var authController = new AuthController(userService);
        var moviesController = new MoviesController(
            new MovieService(context),
            new RatingService(context),
            new CommentService(context),
            new FavoriteService(context),
            new ViewHistoryService(context),
            userService
        );
        var userController = new UserController(
            new FavoriteService(context),
            new ViewHistoryService(context),
            userService
        );

        // 1. Логин
        var loginResult = await authController.Login(new LoginRequest
        {
            Email = "user@test.com",
            Password = "password123"
        });
        var authResponse = (loginResult.Result as OkObjectResult)!.Value as ApiResponse<AuthResponse>;
        var token = authResponse!.Data!.Token;

        // 2. Добавляем в избранное (movie2)
        await moviesController.AddToFavorites(2, token);

        // 3. Получаем избранное
        var favResult = await userController.GetFavorites(token);
        var favOk = favResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var favResponse = favOk.Value.Should().BeOfType<ApiResponse<List<MovieResponse>>>().Subject;
        favResponse.Data.Should().Contain(m => m.Id == 2);
    }
}

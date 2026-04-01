using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.IntegrationTests;

/// <summary>
/// Интеграционные тесты: взаимодействие модулей фильмов и пользовательских действий.
/// Проверяется работа рейтингов, комментариев, избранного, истории просмотров.
/// </summary>
public class MovieUserInteractionIntegrationTests
{
    /// <summary>
    /// Сценарий: пользователь оценивает фильм → рейтинг сохраняется → можно получить через сервис
    /// </summary>
    [Fact]
    public async Task RateMovieThenGetRating_ShouldShowUserRating()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var ratingService = new RatingService(context);
        var moviesController = new MoviesController(
            new MovieService(context), ratingService, new CommentService(context),
            new FavoriteService(context), new ViewHistoryService(context), userService
        );

        // Используем user2 (с подпиской) для оценки movie1 (бесплатный)
        var token = "Bearer_2_user_guid";

        // 1. Переоцениваем movie1 через контроллер (user2 уже оценил на 4, обновляем на 3)
        var rateResult = await moviesController.Rate(1, new RateMovieRequest { Rating = 3 }, token);
        rateResult.Result.Should().BeOfType<OkObjectResult>();

        // 2. Проверяем, что рейтинг обновлён через сервис
        var userRating = await ratingService.GetUserRatingAsync(2, 1);
        userRating.Should().Be(3);
    }

    /// <summary>
    /// Сценарий: запись просмотра → получение истории просмотров
    /// </summary>
    [Fact]
    public async Task RecordViewThenGetHistory_ShouldContainMovie()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var viewHistoryService = new ViewHistoryService(context);
        var moviesController = new MoviesController(
            new MovieService(context), new RatingService(context),
            new CommentService(context), new FavoriteService(context),
            viewHistoryService, userService
        );
        var userController = new UserController(
            new FavoriteService(context), viewHistoryService, userService
        );

        var token = "Bearer_2_user_guid";

        // 1. Записываем просмотр movie1
        await moviesController.RecordView(1, new RecordViewRequest { ProgressSeconds = 1800, Completed = false }, token);

        // 2. Получаем историю
        var historyResult = await userController.GetHistory(token);
        var okResult = historyResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var history = okResult.Value.Should().BeOfType<ApiResponse<List<MovieResponse>>>().Subject;
        history.Data.Should().Contain(m => m.Id == 1);
    }

    /// <summary>
    /// Сценарий: добавление/удаление из избранного → проверка статуса IsFavorite
    /// </summary>
    [Fact]
    public async Task AddRemoveFavorite_ShouldToggleIsFavorite()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var favoriteService = new FavoriteService(context);
        var movieService = new MovieService(context);
        var moviesController = new MoviesController(
            movieService, new RatingService(context),
            new CommentService(context), favoriteService,
            new ViewHistoryService(context), userService
        );

        var token = "Bearer_2_user_guid";

        // 1. Добавляем в избранное
        await moviesController.AddToFavorites(1, token);

        // 2. Проверяем IsFavorite = true
        var movie1 = await movieService.GetByIdAsync(1, 2);
        movie1!.IsFavorite.Should().BeTrue();

        // 3. Удаляем из избранного
        await moviesController.RemoveFromFavorites(1, token);

        // 4. Проверяем IsFavorite = false
        var movie2 = await movieService.GetByIdAsync(1, 2);
        movie2!.IsFavorite.Should().BeFalse();
    }

    /// <summary>
    /// Сценарий: удаление комментария — свой vs чужой
    /// </summary>
    [Fact]
    public async Task DeleteComment_OwnVsOther_ShouldRespectOwnership()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var commentService = new CommentService(context);
        var moviesController = new MoviesController(
            new MovieService(context), new RatingService(context),
            commentService, new FavoriteService(context),
            new ViewHistoryService(context), userService
        );

        // User1 пытается удалить комментарий User2 (id=2) → false
        var result1 = await moviesController.DeleteComment(2, "Bearer_1_user_guid");
        var ok1 = result1.Result.Should().BeOfType<OkObjectResult>().Subject;
        var resp1 = ok1.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        resp1.Data.Should().BeFalse();

        // User1 удаляет свой комментарий (id=1) → true
        var result2 = await moviesController.DeleteComment(1, "Bearer_1_user_guid");
        var ok2 = result2.Result.Should().BeOfType<OkObjectResult>().Subject;
        var resp2 = ok2.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        resp2.Data.Should().BeTrue();
    }
}

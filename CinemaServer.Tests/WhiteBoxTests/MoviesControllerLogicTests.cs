using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.WhiteBoxTests;

/// <summary>
/// Тесты белого ящика для MoviesController.
/// Проверяются все ветви условий в методах контроллера:
/// авторизация, подписка, админские действия.
/// </summary>
public class MoviesControllerLogicTests
{
    // =============================================
    // GetVideo — анализ всех ветвей
    // =============================================

    /// <summary>
    /// Ветвь 1: фильм не найден → NotFound
    /// </summary>
    [Fact]
    public async Task GetVideo_NonExistingMovie_ShouldReturnNotFound()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.GetVideo(999, "Bearer_1_user_guid");

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    /// <summary>
    /// Ветвь 2: бесплатный фильм без авторизации → Ok с URL
    /// </summary>
    [Fact]
    public async Task GetVideo_FreeMovie_NoAuth_ShouldReturnOk()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.GetVideo(1, null); // movie1 — бесплатный

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Ветвь 3: премиум фильм без авторизации → Unauthorized
    /// </summary>
    [Fact]
    public async Task GetVideo_PremiumMovie_NoAuth_ShouldReturnUnauthorized()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.GetVideo(2, null); // movie2 — требует подписку

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Ветвь 4: премиум фильм, пользователь без подписки → 403
    /// </summary>
    [Fact]
    public async Task GetVideo_PremiumMovie_NoSubscription_ShouldReturn403()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.GetVideo(2, "Bearer_1_user_guid"); // user1 — без подписки

        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(403);
    }

    // =============================================
    // Rate — анализ всех ветвей
    // =============================================

    /// <summary>
    /// Ветвь 1: без авторизации → Unauthorized
    /// </summary>
    [Fact]
    public async Task Rate_NoAuth_ShouldReturnUnauthorized()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.Rate(1, new RateMovieRequest { Rating = 5 }, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Ветвь 2: оценка бесплатного фильма → Ok
    /// </summary>
    [Fact]
    public async Task Rate_FreeMovie_ShouldReturnOk()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.Rate(1, new RateMovieRequest { Rating = 4 }, "Bearer_1_user_guid");

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // =============================================
    // AddComment — анализ ветвей
    // =============================================

    /// <summary>
    /// Ветвь 1: без авторизации → Unauthorized
    /// </summary>
    [Fact]
    public async Task AddComment_NoAuth_ShouldReturnUnauthorized()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.AddComment(1, new CreateCommentRequest { Content = "Test" }, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Ветвь 2: с авторизацией → Created
    /// </summary>
    [Fact]
    public async Task AddComment_Authorized_ShouldReturnCreated()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.AddComment(
            1, new CreateCommentRequest { Content = "Отличный фильм!" }, "Bearer_1_user_guid");

        result.Result.Should().BeOfType<CreatedResult>();
    }

    // =============================================
    // DeleteComment — анализ ветвей
    // =============================================

    /// <summary>
    /// Ветвь 1: без авторизации → Unauthorized
    /// </summary>
    [Fact]
    public async Task DeleteComment_NoAuth_ShouldReturnUnauthorized()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.DeleteComment(1, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Ветвь 2: удаление своего комментария → Ok
    /// </summary>
    [Fact]
    public async Task DeleteComment_OwnComment_ShouldReturnOk()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.DeleteComment(1, "Bearer_1_user_guid"); // comment от user1

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Ветвь 3: удаление чужого комментария обычным пользователем → Ok(false)
    /// </summary>
    [Fact]
    public async Task DeleteComment_OtherUsersComment_ShouldReturnOkFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.DeleteComment(2, "Bearer_1_user_guid"); // comment от user2

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        response.Data.Should().BeFalse();
    }

    /// <summary>
    /// Ветвь 4: удаление любого комментария админом → Ok(true)
    /// </summary>
    [Fact]
    public async Task DeleteComment_Admin_ShouldReturnOkTrue()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.DeleteComment(2, "Bearer_3_admin_guid"); // admin может удалять всё

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        response.Data.Should().BeTrue();
    }

    // =============================================
    // Create (Movie) — анализ ветвей
    // =============================================

    /// <summary>
    /// Ветвь 1: обычный пользователь → 403
    /// </summary>
    [Fact]
    public async Task CreateMovie_RegularUser_ShouldReturn403()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.Create(
            new CreateMovieRequest { Title = "X", VideoUrl = "x" }, "Bearer_1_user_guid");

        (result.Result as ObjectResult)!.StatusCode.Should().Be(403);
    }

    /// <summary>
    /// Ветвь 2: админ → CreatedAtAction
    /// </summary>
    [Fact]
    public async Task CreateMovie_Admin_ShouldReturnCreated()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.Create(
            new CreateMovieRequest
            {
                Title = "Новый фильм",
                VideoUrl = "https://example.com/video.mp4",
                ReleaseYear = 2025
            }, "Bearer_3_admin_guid");

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    // =============================================
    // Favorites — ветви
    // =============================================

    /// <summary>
    /// Добавление в избранное без авторизации → Unauthorized
    /// </summary>
    [Fact]
    public async Task AddToFavorites_NoAuth_ShouldReturnUnauthorized()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.AddToFavorites(1, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Удаление из избранного без авторизации → Unauthorized
    /// </summary>
    [Fact]
    public async Task RemoveFromFavorites_NoAuth_ShouldReturnUnauthorized()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.RemoveFromFavorites(1, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Добавление в избранное авторизованным пользователем → Ok
    /// </summary>
    [Fact]
    public async Task AddToFavorites_Authorized_ShouldReturnOk()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.AddToFavorites(2, "Bearer_1_user_guid");

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Удаление из избранного авторизованным пользователем → Ok
    /// </summary>
    [Fact]
    public async Task RemoveFromFavorites_Authorized_ShouldReturnOk()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.RemoveFromFavorites(1, "Bearer_1_user_guid");

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // =============================================
    // RecordView — ветви
    // =============================================

    /// <summary>
    /// Запись просмотра без авторизации → Unauthorized
    /// </summary>
    [Fact]
    public async Task RecordView_NoAuth_ShouldReturnUnauthorized()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.RecordView(1, new RecordViewRequest { ProgressSeconds = 100 }, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Запись просмотра авторизованным пользователем → Ok
    /// </summary>
    [Fact]
    public async Task RecordView_Authorized_ShouldReturnOk()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = CreateController(context);

        var result = await controller.RecordView(1,
            new RecordViewRequest { ProgressSeconds = 1800, Completed = false },
            "Bearer_1_user_guid");

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    private static MoviesController CreateController(CinemaServer.Models.CinemaOnlineContext context)
    {
        var userService = new UserService(context);
        return new MoviesController(
            new MovieService(context),
            new RatingService(context),
            new CommentService(context),
            new FavoriteService(context),
            new ViewHistoryService(context),
            userService);
    }
}

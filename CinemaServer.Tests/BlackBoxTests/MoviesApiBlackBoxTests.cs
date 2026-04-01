using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.BlackBoxTests;

/// <summary>
/// Тесты чёрного ящика для Movies API.
/// Тестируется поведение endpoint'ов фильмов с точки зрения пользователя.
/// </summary>
public class MoviesApiBlackBoxTests
{
    private MoviesController CreateController()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        return new MoviesController(
            new MovieService(context),
            new RatingService(context),
            new CommentService(context),
            new FavoriteService(context),
            new ViewHistoryService(context),
            new UserService(context)
        );
    }

    // =============================================
    // GET /api/movies — поиск фильмов
    // =============================================

    /// <summary>
    /// Получение списка фильмов без фильтров → 200, список фильмов
    /// </summary>
    [Fact]
    public async Task Search_NoFilters_ShouldReturn200WithMovies()
    {
        var controller = CreateController();

        var result = await controller.Search(new MovieSearchRequest());

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<PagedResponse<MovieResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Items.Should().NotBeEmpty();
    }

    /// <summary>
    /// Поиск по названию → корректный результат
    /// </summary>
    [Fact]
    public async Task Search_ByTitle_ShouldReturnMatchingMovies()
    {
        var controller = CreateController();

        var result = await controller.Search(new MovieSearchRequest { Search = "Тестовый" });

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<PagedResponse<MovieResponse>>>().Subject;
        response.Data!.Items.Should().HaveCount(1);
    }

    /// <summary>
    /// Поиск с несуществующим названием → пустой список
    /// </summary>
    [Fact]
    public async Task Search_NonExistingTitle_ShouldReturnEmpty()
    {
        var controller = CreateController();

        var result = await controller.Search(new MovieSearchRequest { Search = "НесуществующийФильм" });

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<PagedResponse<MovieResponse>>>().Subject;
        response.Data!.Items.Should().BeEmpty();
    }

    // =============================================
    // GET /api/movies/{id} — получение фильма
    // =============================================

    /// <summary>
    /// Получение существующего фильма → 200 OK
    /// </summary>
    [Fact]
    public async Task GetById_ExistingMovie_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.GetById(1, null);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<MovieDetailResponse>>().Subject;
        response.Data!.Title.Should().Be("Тестовый фильм 1");
    }

    /// <summary>
    /// Получение несуществующего фильма → 404 Not Found
    /// </summary>
    [Fact]
    public async Task GetById_NonExistingMovie_ShouldReturn404()
    {
        var controller = CreateController();

        var result = await controller.GetById(999, null);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =============================================
    // GET /api/movies/random — случайный фильм
    // =============================================

    /// <summary>
    /// Получение случайного фильма → 200 OK
    /// </summary>
    [Fact]
    public async Task GetRandom_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.GetRandom();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    // =============================================
    // POST /api/movies/{id}/rate — оценка фильма
    // =============================================

    /// <summary>
    /// Оценка фильма авторизованным пользователем → 200 OK
    /// </summary>
    [Fact]
    public async Task Rate_AuthorizedUser_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.Rate(1, new RateMovieRequest { Rating = 4 }, "Bearer_1_user_guid");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        response.Success.Should().BeTrue();
    }

    /// <summary>
    /// Оценка фильма без авторизации → 401
    /// </summary>
    [Fact]
    public async Task Rate_NoAuth_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.Rate(1, new RateMovieRequest { Rating = 4 }, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // =============================================
    // POST /api/movies/{id}/comments — комментарий
    // =============================================

    /// <summary>
    /// Добавление комментария авторизованным пользователем → 201 Created
    /// </summary>
    [Fact]
    public async Task AddComment_AuthorizedUser_ShouldReturn201()
    {
        var controller = CreateController();

        var result = await controller.AddComment(1, new CreateCommentRequest { Content = "Тест" }, "Bearer_1_user_guid");

        var createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    /// <summary>
    /// Добавление комментария без авторизации → 401
    /// </summary>
    [Fact]
    public async Task AddComment_NoAuth_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.AddComment(1, new CreateCommentRequest { Content = "Тест" }, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // =============================================
    // POST /api/movies/{id}/favorite — избранное
    // =============================================

    /// <summary>
    /// Добавление в избранное → 200 OK
    /// </summary>
    [Fact]
    public async Task AddToFavorites_Authorized_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.AddToFavorites(2, "Bearer_1_user_guid");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Добавление в избранное без авторизации → 401
    /// </summary>
    [Fact]
    public async Task AddToFavorites_NoAuth_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.AddToFavorites(1, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // =============================================
    // CRUD — только админ
    // =============================================

    /// <summary>
    /// Создание фильма админом → 201 Created
    /// </summary>
    [Fact]
    public async Task Create_AdminUser_ShouldReturn201()
    {
        var controller = CreateController();
        var request = new CreateMovieRequest
        {
            Title = "Новый фильм",
            VideoUrl = "https://example.com/video.mp4",
            GenreIds = new List<long> { 1 }
        };

        var result = await controller.Create(request, "Bearer_3_admin_guid");

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    /// <summary>
    /// Создание фильма обычным пользователем → 403 Forbidden
    /// </summary>
    [Fact]
    public async Task Create_RegularUser_ShouldReturn403()
    {
        var controller = CreateController();
        var request = new CreateMovieRequest { Title = "X", VideoUrl = "x" };

        var result = await controller.Create(request, "Bearer_1_user_guid");

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(403);
    }

    /// <summary>
    /// Удаление фильма админом → 200 OK
    /// </summary>
    [Fact]
    public async Task Delete_AdminUser_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.Delete(1, "Bearer_3_admin_guid");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Удаление фильма обычным пользователем → 403 Forbidden
    /// </summary>
    [Fact]
    public async Task Delete_RegularUser_ShouldReturn403()
    {
        var controller = CreateController();

        var result = await controller.Delete(1, "Bearer_1_user_guid");

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(403);
    }
}

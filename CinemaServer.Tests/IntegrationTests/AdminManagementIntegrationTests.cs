using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.IntegrationTests;

/// <summary>
/// Интеграционные тесты для административных операций.
/// Проверяется взаимодействие: админ → управление фильмами/жанрами/коллекциями/комментариями.
/// </summary>
public class AdminManagementIntegrationTests
{
    private readonly string _adminToken = "Bearer_3_admin_guid";

    /// <summary>
    /// Полный цикл управления жанрами: создание → обновление → удаление → проверка через API жанров
    /// </summary>
    [Fact]
    public async Task GenreCRUD_FullCycle()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var adminController = new AdminController(context);
        var genreService = new GenreService(context);
        var genresController = new GenresController(genreService);

        // 1. Создаём жанр
        var createResult = await adminController.CreateGenre(
            new GenreRequest { Name = "thriller", DisplayName = "Триллер" }, _adminToken);
        var createOk = createResult.Should().BeOfType<OkObjectResult>().Subject;
        var createData = createOk.Value.Should().BeOfType<ApiResponse<long>>().Subject;
        var genreId = createData.Data;
        genreId.Should().BeGreaterThan(0);

        // 2. Проверяем через API жанров
        var genresResult = await genresController.GetAll();
        var genresOk = genresResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var genresData = genresOk.Value.Should().BeOfType<ApiResponse<List<GenreResponse>>>().Subject;
        genresData.Data.Should().Contain(g => g.Name == "thriller" && g.DisplayName == "Триллер");

        // 3. Обновляем жанр
        await adminController.UpdateGenre(genreId, new GenreRequest { Name = "thriller_updated", DisplayName = "Триллер/Саспенс" }, _adminToken);

        // 4. Проверяем обновление
        var genresAfterUpdate = await genresController.GetAll();
        var genresUpdated = (genresAfterUpdate.Result as OkObjectResult)!.Value as ApiResponse<List<GenreResponse>>;
        genresUpdated!.Data.Should().Contain(g => g.DisplayName == "Триллер/Саспенс");

        // 5. Удаляем жанр
        await adminController.DeleteGenre(genreId, _adminToken);

        // 6. Проверяем удаление
        var genresAfterDelete = await genresController.GetAll();
        var genresDeleted = (genresAfterDelete.Result as OkObjectResult)!.Value as ApiResponse<List<GenreResponse>>;
        genresDeleted!.Data.Should().NotContain(g => g.Name == "thriller_updated");
    }

    /// <summary>
    /// Полный цикл управления коллекциями: создание → добавление фильма → удаление фильма → удаление коллекции
    /// </summary>
    [Fact]
    public async Task CollectionCRUD_FullCycle()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var adminController = new AdminController(context);
        var collectionService = new CollectionService(context);
        var collectionsController = new CollectionsController(collectionService);

        // 1. Создаём коллекцию
        var createResult = await adminController.CreateCollection(new CollectionRequest
        {
            Name = "Новинки месяца",
            Description = "Лучшие новинки текущего месяца",
            IsFeatured = true,
            DisplayOrder = 5
        }, _adminToken);
        var createData = (createResult as OkObjectResult)!.Value as ApiResponse<long>;
        var collectionId = createData!.Data;

        // 2. Добавляем фильмы
        await adminController.AddMovieToCollection(collectionId, new CollectionMovieRequest { MovieId = 1, Position = 1 }, _adminToken);
        await adminController.AddMovieToCollection(collectionId, new CollectionMovieRequest { MovieId = 2, Position = 2 }, _adminToken);

        // 3. Проверяем коллекции через публичный API
        var featured = await collectionsController.GetFeatured();
        var featuredData = (featured.Result as OkObjectResult)!.Value as ApiResponse<List<CollectionResponse>>;
        featuredData!.Data.Should().Contain(c => c.Name == "Новинки месяца");

        // 4. Удаляем фильм из коллекции
        var removeResult = await adminController.RemoveMovieFromCollection(collectionId, 1, _adminToken);
        removeResult.Should().BeOfType<OkObjectResult>();

        // 5. Удаляем коллекцию
        var deleteResult = await adminController.DeleteCollection(collectionId, _adminToken);
        deleteResult.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Полный цикл управления комментариями: просмотр → переключение видимости → удаление
    /// </summary>
    [Fact]
    public async Task CommentManagement_FullCycle()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var adminController = new AdminController(context);
        var commentService = new CommentService(context);

        // 1. Получаем все комментарии
        var allResult = await adminController.GetAllComments(_adminToken);
        allResult.Should().BeOfType<OkObjectResult>();

        // 2. Переключаем видимость комментария 1 (visible → hidden)
        var toggleResult = await adminController.ToggleCommentVisibility(1, _adminToken);
        toggleResult.Should().BeOfType<OkObjectResult>();

        // Проверяем что видимый комментарий стал невидимым
        var comments = await commentService.GetByMovieIdAsync(1);
        // Комментарий 1 теперь не видим в публичном API
        comments.Should().NotContain(c => c.Id == 1);

        // 3. Удаляем комментарий
        var deleteResult = await adminController.DeleteComment(2, _adminToken);
        deleteResult.Should().BeOfType<OkObjectResult>();

        // Проверяем удаление
        var commentsAfter = await commentService.GetByMovieIdAsync(1);
        commentsAfter.Should().NotContain(c => c.Id == 2);
    }

    /// <summary>
    /// Управление пользователями: изменение роли → удаление пользователя
    /// </summary>
    [Fact]
    public async Task UserManagement_RoleChangeAndDelete()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var adminController = new AdminController(context);
        var userService = new UserService(context);

        // 1. Изменяем роль пользователя 1 на admin
        var roleResult = await adminController.UpdateUserRole(1, new UpdateRoleRequest { Role = "admin" }, _adminToken);
        roleResult.Should().BeOfType<OkObjectResult>();

        // 2. Проверяем роль
        var user = await userService.GetByIdAsync(1);
        user!.Role.Should().Be("admin");

        // 3. Удаляем пользователя
        var deleteResult = await adminController.DeleteUser(1, _adminToken);
        deleteResult.Should().BeOfType<OkObjectResult>();

        // 4. Проверяем удаление
        var deletedUser = await userService.GetByIdAsync(1);
        deletedUser.Should().BeNull();
    }

    /// <summary>
    /// Полный CRUD фильма через MoviesController (админские эндпоинты)
    /// </summary>
    [Fact]
    public async Task MovieCRUD_AdminFullCycle()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var movieService = new MovieService(context);
        var moviesController = new MoviesController(
            movieService, new RatingService(context), new CommentService(context),
            new FavoriteService(context), new ViewHistoryService(context), userService);

        // 1. Создаём фильм
        var createResult = await moviesController.Create(new CreateMovieRequest
        {
            Title = "Интеграционный фильм",
            Description = "Описание для интеграционного теста",
            VideoUrl = "https://example.com/integration.mp4",
            ReleaseYear = 2025,
            DurationMinutes = 120,
            Country = "Россия",
            Director = "Тест Режиссёр",
            NeedSubscription = false,
            GenreIds = new List<long> { 1 }
        }, _adminToken);
        var createdData = (createResult.Result as CreatedAtActionResult)!.Value as ApiResponse<long>;
        var movieId = createdData!.Data;

        // 2. Проверяем через поиск
        var searchResult = await moviesController.Search(new MovieSearchRequest { Search = "Интеграционный" });
        var searchData = (searchResult.Result as OkObjectResult)!.Value as ApiResponse<PagedResponse<MovieResponse>>;
        searchData!.Data!.Items.Should().Contain(m => m.Title == "Интеграционный фильм");

        // 3. Обновляем
        var updateResult = await moviesController.Update(movieId, new UpdateMovieRequest
        {
            Title = "Обновлённый интеграционный фильм",
            NeedSubscription = true,
            IsPublished = true
        }, _adminToken);
        updateResult.Result.Should().BeOfType<OkObjectResult>();

        // 4. Проверяем обновление
        var movie = await movieService.GetByIdAsync(movieId);
        movie!.Title.Should().Be("Обновлённый интеграционный фильм");
        movie.NeedSubscription.Should().BeTrue();

        // 5. Удаляем
        var deleteResult = await moviesController.Delete(movieId, _adminToken);
        deleteResult.Result.Should().BeOfType<OkObjectResult>();

        // 6. Проверяем удаление — фильм не найден
        var deleted = await movieService.GetByIdAsync(movieId);
        deleted.Should().BeNull();
    }

    /// <summary>
    /// Создание + изменение видимости комментария + проверка в публичном API
    /// </summary>
    [Fact]
    public async Task Comment_CreateByUser_ToggleVisibilityByAdmin()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var commentService = new CommentService(context);
        var moviesController = new MoviesController(
            new MovieService(context), new RatingService(context), commentService,
            new FavoriteService(context), new ViewHistoryService(context), userService);
        var adminController = new AdminController(context);

        // 1. Пользователь создаёт комментарий
        var commentResult = await moviesController.AddComment(
            1, new CreateCommentRequest { Content = "Новый комментарий от пользователя" },
            "Bearer_1_user_guid");
        var commentData = (commentResult.Result as CreatedResult)!.Value as ApiResponse<long>;
        var commentId = commentData!.Data;

        // 2. Комментарий виден в публичном API
        var comments = await commentService.GetByMovieIdAsync(1);
        comments.Should().Contain(c => c.Id == commentId);

        // 3. Админ скрывает комментарий
        await adminController.ToggleCommentVisibility(commentId, _adminToken);

        // 4. Комментарий больше НЕ виден в публичном API
        var commentsAfter = await commentService.GetByMovieIdAsync(1);
        commentsAfter.Should().NotContain(c => c.Id == commentId);
    }
}

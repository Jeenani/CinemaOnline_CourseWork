using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.SystemTests;

/// <summary>
/// Системные тесты — полные сценарии администрирования «от начала до конца».
/// Проверяется работа всех компонентов админ-панели вместе.
/// </summary>
public class AdminJourneySystemTests
{
    private readonly string _adminToken = "Bearer_3_admin_guid";

    /// <summary>
    /// Полный путь администратора:
    /// Просмотр статистики → Управление жанрами → Создание фильма → 
    /// Управление коллекциями → Модерация комментариев → Управление пользователями
    /// </summary>
    [Fact]
    public async Task FullAdminJourney_ContentManagement()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var movieService = new MovieService(context);
        var subService = new SubscriptionService(context);
        var commentService = new CommentService(context);
        var genreService = new GenreService(context);
        var collectionService = new CollectionService(context);

        var adminController = new AdminController(context);
        var moviesController = new MoviesController(
            movieService, new RatingService(context), commentService,
            new FavoriteService(context), new ViewHistoryService(context), userService);
        var genresController = new GenresController(genreService);
        var collectionsController = new CollectionsController(collectionService);
        var subsController = new SubscriptionsController(subService);

        // ========== ШАГ 1: Просмотр статистики ==========
        var statsResult = await adminController.GetStats(_adminToken);
        statsResult.Should().BeOfType<OkObjectResult>();

        // ========== ШАГ 2: Управление жанрами ==========
        // Создаём новый жанр
        var genreResult = await adminController.CreateGenre(
            new GenreRequest { Name = "sci-fi", DisplayName = "Фантастика" }, _adminToken);
        var genreData = (genreResult as OkObjectResult)!.Value as ApiResponse<long>;
        var sciFiId = genreData!.Data;
        sciFiId.Should().BeGreaterThan(0);

        // Проверяем через публичный API
        var genres = await genresController.GetAll();
        var genresData = (genres.Result as OkObjectResult)!.Value as ApiResponse<List<GenreResponse>>;
        genresData!.Data.Should().Contain(g => g.Name == "sci-fi");

        // ========== ШАГ 3: Создание фильма с новым жанром ==========
        var createResult = await moviesController.Create(new CreateMovieRequest
        {
            Title = "Звёздные войны нового поколения",
            Description = "Эпическая космическая сага",
            VideoUrl = "https://example.com/starwars.mp4",
            VkVideoUrl = "https://vk.com/starwars",
            PosterUrl = "/posters/starwars.jpg",
            BannerUrl = "/banners/starwars.jpg",
            ReleaseYear = 2025,
            DurationMinutes = 150,
            Country = "США",
            Director = "Джордж Лукас мл.",
            NeedSubscription = true,
            GenreIds = new List<long> { sciFiId, 1 } // sci-fi + action
        }, _adminToken);
        var movieCreated = (createResult.Result as CreatedAtActionResult)!.Value as ApiResponse<long>;
        var newMovieId = movieCreated!.Data;

        // Проверяем через поиск
        var searchResult = await moviesController.Search(new MovieSearchRequest { Search = "Звёздные" });
        var searchData = (searchResult.Result as OkObjectResult)!.Value as ApiResponse<PagedResponse<MovieResponse>>;
        searchData!.Data!.Items.Should().Contain(m => m.Title == "Звёздные войны нового поколения");

        // ========== ШАГ 4: Создание коллекции и добавление фильма ==========
        var collResult = await adminController.CreateCollection(new CollectionRequest
        {
            Name = "Лучшая фантастика",
            Description = "Топ фантастических фильмов",
            IsFeatured = true,
            DisplayOrder = 10
        }, _adminToken);
        var collData = (collResult as OkObjectResult)!.Value as ApiResponse<long>;
        var collId = collData!.Data;

        // Добавляем фильм в коллекцию
        await adminController.AddMovieToCollection(collId, new CollectionMovieRequest
        {
            MovieId = newMovieId, Position = 1
        }, _adminToken);

        // Проверяем через публичный API
        var featured = await collectionsController.GetFeatured();
        var featuredData = (featured.Result as OkObjectResult)!.Value as ApiResponse<List<CollectionResponse>>;
        featuredData!.Data.Should().Contain(c => c.Name == "Лучшая фантастика");

        // ========== ШАГ 5: Модерация комментариев ==========
        var commentsResult = await adminController.GetAllComments(_adminToken);
        commentsResult.Should().BeOfType<OkObjectResult>();

        // Скрываем первый комментарий
        await adminController.ToggleCommentVisibility(1, _adminToken);

        // Удаляем скрытый комментарий (id=3)
        await adminController.DeleteComment(3, _adminToken);

        // ========== ШАГ 6: Управление пользователями ==========
        var usersResult = await adminController.GetUsers(_adminToken);
        usersResult.Should().BeOfType<OkObjectResult>();

        // ========== ШАГ 7: Проверяем получение всех фильмов (включая неопубликованные) ==========
        var allMovies = await adminController.GetAllMovies(_adminToken);
        allMovies.Should().BeOfType<OkObjectResult>();

        // ========== ШАГ 8: Обновляем фильм ==========
        var updateResult = await moviesController.Update(newMovieId, new UpdateMovieRequest
        {
            Title = "Звёздные войны: Финал",
            IsPublished = true
        }, _adminToken);
        updateResult.Result.Should().BeOfType<OkObjectResult>();

        // Проверяем обновление
        var updatedMovie = await movieService.GetByIdAsync(newMovieId);
        updatedMovie!.Title.Should().Be("Звёздные войны: Финал");

        // ========== ШАГ 9: Статистика после всех операций ==========
        var statsAfter = await adminController.GetStats(_adminToken);
        statsAfter.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Сценарий безопасности: все админские эндпоинты блокируют обычного пользователя
    /// </summary>
    [Fact]
    public async Task SecurityScenario_AllAdminEndpoints_BlockedForUser()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var adminController = new AdminController(context);
        var userToken = "Bearer_1_user_guid";

        // Статистика
        (await adminController.GetStats(userToken)).Should().BeOfType<UnauthorizedObjectResult>();

        // Пользователи
        (await adminController.GetUsers(userToken)).Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.UpdateUserRole(1, new UpdateRoleRequest { Role = "admin" }, userToken))
            .Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.DeleteUser(1, userToken)).Should().BeOfType<UnauthorizedObjectResult>();

        // Жанры
        (await adminController.CreateGenre(new GenreRequest { Name = "x", DisplayName = "X" }, userToken))
            .Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.UpdateGenre(1, new GenreRequest { Name = "x", DisplayName = "X" }, userToken))
            .Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.DeleteGenre(1, userToken)).Should().BeOfType<UnauthorizedObjectResult>();

        // Коллекции
        (await adminController.GetCollections(userToken)).Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.CreateCollection(new CollectionRequest { Name = "x" }, userToken))
            .Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.UpdateCollection(1, new CollectionRequest { Name = "x" }, userToken))
            .Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.DeleteCollection(1, userToken)).Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.AddMovieToCollection(1, new CollectionMovieRequest { MovieId = 1 }, userToken))
            .Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.RemoveMovieFromCollection(1, 1, userToken))
            .Should().BeOfType<UnauthorizedObjectResult>();

        // Комментарии
        (await adminController.GetAllComments(userToken)).Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.ToggleCommentVisibility(1, userToken)).Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.DeleteComment(1, userToken)).Should().BeOfType<UnauthorizedObjectResult>();

        // Фильмы
        (await adminController.GetAllMovies(userToken)).Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Сценарий безопасности: все админские эндпоинты блокируют запросы без токена
    /// </summary>
    [Fact]
    public async Task SecurityScenario_AllAdminEndpoints_BlockedWithoutToken()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var adminController = new AdminController(context);

        (await adminController.GetStats(null)).Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.GetUsers(null)).Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.GetAllComments(null)).Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.GetCollections(null)).Should().BeOfType<UnauthorizedObjectResult>();
        (await adminController.GetAllMovies(null)).Should().BeOfType<UnauthorizedObjectResult>();
    }
}

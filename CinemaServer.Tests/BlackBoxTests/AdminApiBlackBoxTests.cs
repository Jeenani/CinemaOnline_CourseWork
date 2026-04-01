using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Models;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.BlackBoxTests;

/// <summary>
/// Тесты чёрного ящика для API администрирования (AdminController).
/// Покрываются все CRUD операции: статистика, пользователи, жанры, коллекции, комментарии, фильмы.
/// </summary>
public class AdminApiBlackBoxTests
{
    private readonly string _adminToken = "Bearer_3_admin_guid";
    private readonly string _userToken = "Bearer_1_user_guid";

    private AdminController CreateController()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        return new AdminController(context);
    }

    // =============================================
    // Статистика — тесты чёрного ящика
    // =============================================

    /// <summary>
    /// Получение статистики администратором → 200 OK со всеми полями
    /// </summary>
    [Fact]
    public async Task GetStats_Admin_ShouldReturn200WithStats()
    {
        var controller = CreateController();

        var result = await controller.GetStats(_adminToken);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Получение статистики обычным пользователем → 401
    /// </summary>
    [Fact]
    public async Task GetStats_RegularUser_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.GetStats(_userToken);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Получение статистики без авторизации → 401
    /// </summary>
    [Fact]
    public async Task GetStats_NoAuth_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.GetStats(null);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // =============================================
    // Управление пользователями — тесты
    // =============================================

    /// <summary>
    /// Получение списка пользователей администратором → 200 OK
    /// </summary>
    [Fact]
    public async Task GetUsers_Admin_ShouldReturn200WithUsers()
    {
        var controller = CreateController();

        var result = await controller.GetUsers(_adminToken);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Получение списка пользователей обычным пользователем → 401
    /// </summary>
    [Fact]
    public async Task GetUsers_RegularUser_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.GetUsers(_userToken);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Изменение роли пользователя → 200 OK
    /// </summary>
    [Fact]
    public async Task UpdateUserRole_Admin_ShouldReturn200()
    {
        var controller = CreateController();
        var request = new UpdateRoleRequest { Role = "admin" };

        var result = await controller.UpdateUserRole(1, request, _adminToken);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Изменение роли несуществующего пользователя → 404
    /// </summary>
    [Fact]
    public async Task UpdateUserRole_NonExistingUser_ShouldReturn404()
    {
        var controller = CreateController();
        var request = new UpdateRoleRequest { Role = "admin" };

        var result = await controller.UpdateUserRole(999, request, _adminToken);

        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Изменение роли обычным пользователем → 401
    /// </summary>
    [Fact]
    public async Task UpdateUserRole_RegularUser_ShouldReturn401()
    {
        var controller = CreateController();
        var request = new UpdateRoleRequest { Role = "admin" };

        var result = await controller.UpdateUserRole(1, request, _userToken);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Удаление пользователя администратором → 200 OK
    /// </summary>
    [Fact]
    public async Task DeleteUser_Admin_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.DeleteUser(1, _adminToken);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Удаление несуществующего пользователя → 404
    /// </summary>
    [Fact]
    public async Task DeleteUser_NonExistingUser_ShouldReturn404()
    {
        var controller = CreateController();

        var result = await controller.DeleteUser(999, _adminToken);

        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Удаление пользователя обычным пользователем → 401
    /// </summary>
    [Fact]
    public async Task DeleteUser_RegularUser_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.DeleteUser(1, _userToken);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // =============================================
    // Управление жанрами — тесты
    // =============================================

    /// <summary>
    /// Создание жанра администратором → 200 OK с Id
    /// </summary>
    [Fact]
    public async Task CreateGenre_Admin_ShouldReturn200WithId()
    {
        var controller = CreateController();
        var request = new GenreRequest { Name = "horror", DisplayName = "Ужасы" };

        var result = await controller.CreateGenre(request, _adminToken);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<long>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Создание жанра обычным пользователем → 401
    /// </summary>
    [Fact]
    public async Task CreateGenre_RegularUser_ShouldReturn401()
    {
        var controller = CreateController();
        var request = new GenreRequest { Name = "horror", DisplayName = "Ужасы" };

        var result = await controller.CreateGenre(request, _userToken);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Обновление жанра администратором → 200 OK
    /// </summary>
    [Fact]
    public async Task UpdateGenre_Admin_ShouldReturn200()
    {
        var controller = CreateController();
        var request = new GenreRequest { Name = "action_updated", DisplayName = "Экшн" };

        var result = await controller.UpdateGenre(1, request, _adminToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Обновление несуществующего жанра → 404
    /// </summary>
    [Fact]
    public async Task UpdateGenre_NonExisting_ShouldReturn404()
    {
        var controller = CreateController();
        var request = new GenreRequest { Name = "test", DisplayName = "Тест" };

        var result = await controller.UpdateGenre(999, request, _adminToken);

        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Удаление жанра администратором → 200 OK
    /// </summary>
    [Fact]
    public async Task DeleteGenre_Admin_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.DeleteGenre(3, _adminToken); // comedy

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Удаление несуществующего жанра → 404
    /// </summary>
    [Fact]
    public async Task DeleteGenre_NonExisting_ShouldReturn404()
    {
        var controller = CreateController();

        var result = await controller.DeleteGenre(999, _adminToken);

        result.Should().BeOfType<NotFoundResult>();
    }

    // =============================================
    // Управление коллекциями — тесты
    // =============================================

    /// <summary>
    /// Получение списка коллекций администратором → 200 OK
    /// </summary>
    [Fact]
    public async Task GetCollections_Admin_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.GetCollections(_adminToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Получение списка коллекций обычным пользователем → 401
    /// </summary>
    [Fact]
    public async Task GetCollections_RegularUser_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.GetCollections(_userToken);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Создание коллекции администратором → 200 OK
    /// </summary>
    [Fact]
    public async Task CreateCollection_Admin_ShouldReturn200WithId()
    {
        var controller = CreateController();
        var request = new CollectionRequest
        {
            Name = "Новая коллекция",
            Description = "Описание коллекции",
            IsFeatured = true,
            DisplayOrder = 2
        };

        var result = await controller.CreateCollection(request, _adminToken);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<long>>().Subject;
        response.Data.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Обновление коллекции администратором → 200 OK
    /// </summary>
    [Fact]
    public async Task UpdateCollection_Admin_ShouldReturn200()
    {
        var controller = CreateController();
        var request = new CollectionRequest
        {
            Name = "Обновлённая коллекция",
            Description = "Новое описание",
            IsFeatured = false,
            DisplayOrder = 3
        };

        var result = await controller.UpdateCollection(1, request, _adminToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Обновление несуществующей коллекции → 404
    /// </summary>
    [Fact]
    public async Task UpdateCollection_NonExisting_ShouldReturn404()
    {
        var controller = CreateController();
        var request = new CollectionRequest { Name = "Test" };

        var result = await controller.UpdateCollection(999, request, _adminToken);

        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Удаление коллекции администратором → 200 OK
    /// </summary>
    [Fact]
    public async Task DeleteCollection_Admin_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.DeleteCollection(1, _adminToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Удаление несуществующей коллекции → 404
    /// </summary>
    [Fact]
    public async Task DeleteCollection_NonExisting_ShouldReturn404()
    {
        var controller = CreateController();

        var result = await controller.DeleteCollection(999, _adminToken);

        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Добавление фильма в коллекцию → 200 OK
    /// </summary>
    [Fact]
    public async Task AddMovieToCollection_Admin_ShouldReturn200()
    {
        var controller = CreateController();
        var request = new CollectionMovieRequest { MovieId = 2, Position = 2 };

        var result = await controller.AddMovieToCollection(1, request, _adminToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Добавление уже существующего фильма в коллекцию → 200 OK (идемпотентность)
    /// </summary>
    [Fact]
    public async Task AddMovieToCollection_AlreadyExists_ShouldReturn200()
    {
        var controller = CreateController();
        var request = new CollectionMovieRequest { MovieId = 1, Position = 1 };

        var result = await controller.AddMovieToCollection(1, request, _adminToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Удаление фильма из коллекции → 200 OK
    /// </summary>
    [Fact]
    public async Task RemoveMovieFromCollection_Admin_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.RemoveMovieFromCollection(1, 1, _adminToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Удаление несуществующего фильма из коллекции → 404
    /// </summary>
    [Fact]
    public async Task RemoveMovieFromCollection_NonExisting_ShouldReturn404()
    {
        var controller = CreateController();

        var result = await controller.RemoveMovieFromCollection(1, 999, _adminToken);

        result.Should().BeOfType<NotFoundResult>();
    }

    // =============================================
    // Управление комментариями — тесты
    // =============================================

    /// <summary>
    /// Получение всех комментариев администратором → 200 OK
    /// </summary>
    [Fact]
    public async Task GetAllComments_Admin_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.GetAllComments(_adminToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Получение всех комментариев обычным пользователем → 401
    /// </summary>
    [Fact]
    public async Task GetAllComments_RegularUser_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.GetAllComments(_userToken);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Переключение видимости комментария → 200 OK
    /// </summary>
    [Fact]
    public async Task ToggleCommentVisibility_Admin_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.ToggleCommentVisibility(1, _adminToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Переключение видимости несуществующего комментария → 404
    /// </summary>
    [Fact]
    public async Task ToggleCommentVisibility_NonExisting_ShouldReturn404()
    {
        var controller = CreateController();

        var result = await controller.ToggleCommentVisibility(999, _adminToken);

        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Удаление комментария администратором → 200 OK
    /// </summary>
    [Fact]
    public async Task DeleteComment_Admin_ShouldReturn200()
    {
        var controller = CreateController();

        var result = await controller.DeleteComment(1, _adminToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Удаление несуществующего комментария → 404
    /// </summary>
    [Fact]
    public async Task DeleteComment_NonExisting_ShouldReturn404()
    {
        var controller = CreateController();

        var result = await controller.DeleteComment(999, _adminToken);

        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Удаление комментария обычным пользователем → 401
    /// </summary>
    [Fact]
    public async Task DeleteComment_RegularUser_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.DeleteComment(1, _userToken);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // =============================================
    // Управление фильмами — тесты
    // =============================================

    /// <summary>
    /// Получение всех фильмов (включая неопубликованные) администратором → 200 OK
    /// </summary>
    [Fact]
    public async Task GetAllMovies_Admin_ShouldReturn200WithAllMovies()
    {
        var controller = CreateController();

        var result = await controller.GetAllMovies(_adminToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Получение всех фильмов обычным пользователем → 401
    /// </summary>
    [Fact]
    public async Task GetAllMovies_RegularUser_ShouldReturn401()
    {
        var controller = CreateController();

        var result = await controller.GetAllMovies(_userToken);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}

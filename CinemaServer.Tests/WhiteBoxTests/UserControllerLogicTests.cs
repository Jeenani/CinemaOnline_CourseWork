using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.WhiteBoxTests;

/// <summary>
/// Тесты белого ящика для UserController.
/// Проверяются все ветви условий (branch coverage) в методах контроллера.
/// </summary>
public class UserControllerLogicTests
{
    // =============================================
    // UpdateProfile — анализ всех ветвей
    // =============================================

    /// <summary>
    /// Ветвь 1: authorization = null → Unauthorized
    /// </summary>
    [Fact]
    public async Task UpdateProfile_NullAuth_Branch_ShouldReturnUnauthorized()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), new UserService(context));

        var result = await controller.UpdateProfile(
            new UpdateProfileRequest { Name = "A", Email = "a@a.com" }, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Ветвь 2: Name пустое → BadRequest
    /// </summary>
    [Fact]
    public async Task UpdateProfile_EmptyName_Branch_ShouldReturnBadRequest()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), new UserService(context));

        var result = await controller.UpdateProfile(
            new UpdateProfileRequest { Name = "   ", Email = "a@a.com" }, "Bearer_1_user_guid");

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Ветвь 3: Email пустой → BadRequest
    /// </summary>
    [Fact]
    public async Task UpdateProfile_EmptyEmail_Branch_ShouldReturnBadRequest()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), new UserService(context));

        var result = await controller.UpdateProfile(
            new UpdateProfileRequest { Name = "Имя", Email = "  " }, "Bearer_1_user_guid");

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Ветвь 4: Email занят другим пользователем → BadRequest
    /// </summary>
    [Fact]
    public async Task UpdateProfile_DuplicateEmail_Branch_ShouldReturnBadRequest()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), new UserService(context));

        var result = await controller.UpdateProfile(
            new UpdateProfileRequest { Name = "Имя", Email = "premium@test.com" }, "Bearer_1_user_guid");

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Ветвь 5: Все данные корректны → Ok
    /// </summary>
    [Fact]
    public async Task UpdateProfile_ValidData_Branch_ShouldReturnOk()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), new UserService(context));

        var result = await controller.UpdateProfile(
            new UpdateProfileRequest { Name = "Новое Имя", Email = "newemail@test.com" }, "Bearer_1_user_guid");

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // =============================================
    // ChangePassword — анализ всех ветвей
    // =============================================

    /// <summary>
    /// Ветвь 1: authorization = null → Unauthorized
    /// </summary>
    [Fact]
    public async Task ChangePassword_NullAuth_Branch_ShouldReturnUnauthorized()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), new UserService(context));

        var result = await controller.ChangePassword(
            new ChangePasswordRequest { CurrentPassword = "x", NewPassword = "newpass" }, null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Ветвь 2: NewPassword слишком короткий → BadRequest
    /// </summary>
    [Fact]
    public async Task ChangePassword_ShortPassword_Branch_ShouldReturnBadRequest()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), new UserService(context));

        var result = await controller.ChangePassword(
            new ChangePasswordRequest { CurrentPassword = "password123", NewPassword = "abc" },
            "Bearer_1_user_guid");

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Ветвь 3: Неверный текущий пароль → BadRequest
    /// </summary>
    [Fact]
    public async Task ChangePassword_WrongCurrent_Branch_ShouldReturnBadRequest()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), new UserService(context));

        var result = await controller.ChangePassword(
            new ChangePasswordRequest { CurrentPassword = "wrong", NewPassword = "newpass123" },
            "Bearer_1_user_guid");

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Ветвь 4: Все данные корректны → Ok
    /// </summary>
    [Fact]
    public async Task ChangePassword_Valid_Branch_ShouldReturnOk()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), new UserService(context));

        var result = await controller.ChangePassword(
            new ChangePasswordRequest { CurrentPassword = "password123", NewPassword = "newpass123" },
            "Bearer_1_user_guid");

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // =============================================
    // GetSubscription — анализ ветвей
    // =============================================

    /// <summary>
    /// Ветвь 1: нет авторизации → Unauthorized
    /// </summary>
    [Fact]
    public async Task GetSubscription_NullAuth_Branch_ShouldReturnUnauthorized()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), new UserService(context));

        var result = await controller.GetSubscription(null);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Ветвь 2: пользователь не найден → Unauthorized
    /// </summary>
    [Fact]
    public async Task GetSubscription_NonExistingUser_Branch_ShouldReturnUnauthorized()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), new UserService(context));

        var result = await controller.GetSubscription("Bearer_999_user_guid");

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Ветвь 3: пользователь найден → Ok с подпиской
    /// </summary>
    [Fact]
    public async Task GetSubscription_ValidUser_Branch_ShouldReturnOk()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), new UserService(context));

        var result = await controller.GetSubscription("Bearer_2_user_guid");

        result.Result.Should().BeOfType<OkObjectResult>();
    }
}

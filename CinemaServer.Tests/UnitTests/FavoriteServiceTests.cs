using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.UnitTests;

/// <summary>
/// Модульные тесты для FavoriteService.
/// Тестируется добавление, удаление и получение избранных фильмов.
/// </summary>
public class FavoriteServiceTests
{
    /// <summary>
    /// Добавление фильма в избранное
    /// </summary>
    [Fact]
    public async Task AddAsync_NewFavorite_ShouldAddAndReturnTrue()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new FavoriteService(context);

        var result = await service.AddAsync(1, 2); // user1 добавляет movie2

        result.Should().BeTrue();
    }

    /// <summary>
    /// Повторное добавление уже существующего избранного — идемпотентность
    /// </summary>
    [Fact]
    public async Task AddAsync_AlreadyExists_ShouldReturnTrue()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new FavoriteService(context);

        var result = await service.AddAsync(1, 1); // user1 уже добавил movie1

        result.Should().BeTrue();
    }

    /// <summary>
    /// Удаление из избранного — успех
    /// </summary>
    [Fact]
    public async Task RemoveAsync_ExistingFavorite_ShouldReturnTrue()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new FavoriteService(context);

        var result = await service.RemoveAsync(1, 1);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Удаление несуществующего избранного — false
    /// </summary>
    [Fact]
    public async Task RemoveAsync_NonExistingFavorite_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new FavoriteService(context);

        var result = await service.RemoveAsync(1, 999);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Получение избранных фильмов пользователя
    /// </summary>
    [Fact]
    public async Task GetUserFavoritesAsync_ShouldReturnFavoriteMovies()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new FavoriteService(context);

        var favorites = await service.GetUserFavoritesAsync(1);

        favorites.Should().HaveCount(1);
        favorites[0].Title.Should().Be("Тестовый фильм 1");
    }

    /// <summary>
    /// Получение избранного у пользователя без избранного — пустой список
    /// </summary>
    [Fact]
    public async Task GetUserFavoritesAsync_NoFavorites_ShouldReturnEmpty()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new FavoriteService(context);

        var favorites = await service.GetUserFavoritesAsync(3); // admin без избранного

        favorites.Should().BeEmpty();
    }
}

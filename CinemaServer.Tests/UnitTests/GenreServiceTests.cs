using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.UnitTests;

/// <summary>
/// Модульные тесты для GenreService.
/// Тестируется получение списка жанров.
/// </summary>
public class GenreServiceTests
{
    /// <summary>
    /// Получение всех жанров — возвращает все записи
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllGenres()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new GenreService(context);

        var genres = await service.GetAllAsync();

        genres.Should().HaveCount(3);
    }

    /// <summary>
    /// Жанры отсортированы по DisplayName
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ShouldBeSortedByDisplayName()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new GenreService(context);

        var genres = await service.GetAllAsync();

        genres.Should().BeInAscendingOrder(g => g.DisplayName);
    }

    /// <summary>
    /// Пустая БД — пустой список
    /// </summary>
    [Fact]
    public async Task GetAllAsync_EmptyDb_ShouldReturnEmpty()
    {
        var context = TestDbContextFactory.Create();
        var service = new GenreService(context);

        var genres = await service.GetAllAsync();

        genres.Should().BeEmpty();
    }
}

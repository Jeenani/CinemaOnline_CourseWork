using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.UnitTests;

/// <summary>
/// Модульные тесты для CollectionService.
/// Тестируется получение избранных коллекций.
/// </summary>
public class CollectionServiceTests
{
    /// <summary>
    /// Получение избранных коллекций — возвращает только featured
    /// </summary>
    [Fact]
    public async Task GetFeaturedAsync_ShouldReturnFeaturedCollections()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new CollectionService(context);

        var collections = await service.GetFeaturedAsync();

        collections.Should().HaveCount(1);
        collections[0].Name.Should().Be("Лучшее за неделю");
    }

    /// <summary>
    /// Избранная коллекция содержит фильмы
    /// </summary>
    [Fact]
    public async Task GetFeaturedAsync_ShouldIncludeMovies()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new CollectionService(context);

        var collections = await service.GetFeaturedAsync();

        collections[0].Movies.Should().NotBeEmpty();
        collections[0].Movies[0].Title.Should().Be("Тестовый фильм 1");
    }

    /// <summary>
    /// Пустая БД — пустой список
    /// </summary>
    [Fact]
    public async Task GetFeaturedAsync_EmptyDb_ShouldReturnEmpty()
    {
        var context = TestDbContextFactory.Create();
        var service = new CollectionService(context);

        var collections = await service.GetFeaturedAsync();

        collections.Should().BeEmpty();
    }
}

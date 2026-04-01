using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.UnitTests;

/// <summary>
/// Модульные тесты для RatingService.
/// Тестируется создание, обновление, получение и удаление рейтингов.
/// </summary>
public class RatingServiceTests
{
    /// <summary>
    /// Новая оценка — создаёт запись
    /// </summary>
    [Fact]
    public async Task RateMovieAsync_NewRating_ShouldCreateRating()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new RatingService(context);

        var result = await service.RateMovieAsync(1, 2, 4); // user1 оценивает movie2

        result.Should().BeTrue();
        var rating = await service.GetUserRatingAsync(1, 2);
        rating.Should().Be(4);
    }

    /// <summary>
    /// Повторная оценка — обновляет существующую
    /// </summary>
    [Fact]
    public async Task RateMovieAsync_ExistingRating_ShouldUpdateRating()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new RatingService(context);

        // user1 уже оценил movie1 на 5, обновляем на 3
        var result = await service.RateMovieAsync(1, 1, 3);

        result.Should().BeTrue();
        var rating = await service.GetUserRatingAsync(1, 1);
        rating.Should().Be(3);
    }

    /// <summary>
    /// Получение рейтинга пользователя для фильма
    /// </summary>
    [Fact]
    public async Task GetUserRatingAsync_ExistingRating_ShouldReturnValue()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new RatingService(context);

        var rating = await service.GetUserRatingAsync(1, 1);

        rating.Should().Be(5);
    }

    /// <summary>
    /// Получение рейтинга — если нет оценки, null
    /// </summary>
    [Fact]
    public async Task GetUserRatingAsync_NoRating_ShouldReturnNull()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new RatingService(context);

        var rating = await service.GetUserRatingAsync(1, 2); // user1 не оценивал movie2

        rating.Should().BeNull();
    }

    /// <summary>
    /// Удаление рейтинга — успех
    /// </summary>
    [Fact]
    public async Task DeleteRatingAsync_ExistingRating_ShouldReturnTrue()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new RatingService(context);

        var result = await service.DeleteRatingAsync(1, 1);

        result.Should().BeTrue();
        var rating = await service.GetUserRatingAsync(1, 1);
        rating.Should().BeNull();
    }

    /// <summary>
    /// Удаление несуществующего рейтинга — false
    /// </summary>
    [Fact]
    public async Task DeleteRatingAsync_NonExistingRating_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new RatingService(context);

        var result = await service.DeleteRatingAsync(1, 999);

        result.Should().BeFalse();
    }
}

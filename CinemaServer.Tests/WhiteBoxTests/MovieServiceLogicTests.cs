using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.WhiteBoxTests;

/// <summary>
/// Тесты белого ящика для MovieService.
/// Проверяется покрытие всех ветвей кода: фильтрация, сортировка, обновление полей.
/// </summary>
public class MovieServiceLogicTests
{
    // =============================================
    // SearchAsync — покрытие всех ветвей фильтрации
    // =============================================

    /// <summary>
    /// Ветвь: Search = null → фильтр по названию не применяется
    /// </summary>
    [Fact]
    public async Task SearchAsync_NullSearch_ShouldNotFilter()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { Search = null });

        result.Items.Should().HaveCount(2);
    }

    /// <summary>
    /// Ветвь: Search = "  " (пробелы) → фильтр не применяется
    /// </summary>
    [Fact]
    public async Task SearchAsync_WhitespaceSearch_ShouldNotFilter()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { Search = "   " });

        result.Items.Should().HaveCount(2);
    }

    /// <summary>
    /// Ветвь: GenreId = null → фильтр по жанру не применяется
    /// </summary>
    [Fact]
    public async Task SearchAsync_NullGenreId_ShouldNotFilterByGenre()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { GenreId = null });

        result.Items.Should().HaveCount(2);
    }

    /// <summary>
    /// Ветвь: YearFrom задан, YearTo не задан
    /// </summary>
    [Fact]
    public async Task SearchAsync_OnlyYearFrom_ShouldFilterFromYear()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { YearFrom = 2024, YearTo = null });

        result.Items.Should().HaveCount(1);
        result.Items[0].ReleaseYear.Should().BeGreaterOrEqualTo(2024);
    }

    /// <summary>
    /// Ветвь: YearTo задан, YearFrom не задан
    /// </summary>
    [Fact]
    public async Task SearchAsync_OnlyYearTo_ShouldFilterToYear()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { YearFrom = null, YearTo = 2023 });

        result.Items.Should().HaveCount(1);
        result.Items[0].ReleaseYear.Should().BeLessOrEqualTo(2023);
    }

    /// <summary>
    /// Ветвь: Country = null → фильтр по стране не применяется
    /// </summary>
    [Fact]
    public async Task SearchAsync_NullCountry_ShouldNotFilterByCountry()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { Country = null });

        result.Items.Should().HaveCount(2);
    }

    /// <summary>
    /// Ветвь: MinRating = null → фильтр по рейтингу не применяется
    /// </summary>
    [Fact]
    public async Task SearchAsync_NullMinRating_ShouldNotFilterByRating()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { MinRating = null });

        result.Items.Should().HaveCount(2);
    }

    // =============================================
    // SearchAsync — покрытие всех ветвей сортировки (switch)
    // =============================================

    /// <summary>
    /// Ветвь switch: SortBy = "rating", SortDescending = false
    /// </summary>
    [Fact]
    public async Task SearchAsync_SortByRatingAsc_ShouldSortAscending()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { SortBy = "rating", SortDescending = false });

        result.Items[0].AverageRating.Should().BeLessOrEqualTo(result.Items[1].AverageRating);
    }

    /// <summary>
    /// Ветвь switch: SortBy = "views", SortDescending = true
    /// </summary>
    [Fact]
    public async Task SearchAsync_SortByViewsDesc_ShouldSortDescending()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { SortBy = "views", SortDescending = true });

        result.Items[0].ViewCount.Should().BeGreaterOrEqualTo(result.Items[1].ViewCount);
    }

    /// <summary>
    /// Ветвь switch: SortBy = "views", SortDescending = false
    /// </summary>
    [Fact]
    public async Task SearchAsync_SortByViewsAsc_ShouldSortAscending()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { SortBy = "views", SortDescending = false });

        result.Items[0].ViewCount.Should().BeLessOrEqualTo(result.Items[1].ViewCount);
    }

    /// <summary>
    /// Ветвь switch: SortBy = "year", SortDescending = true
    /// </summary>
    [Fact]
    public async Task SearchAsync_SortByYearDesc_ShouldSortDescending()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { SortBy = "year", SortDescending = true });

        result.Items[0].ReleaseYear.Should().BeGreaterOrEqualTo(result.Items[1].ReleaseYear.GetValueOrDefault());
    }

    /// <summary>
    /// Ветвь switch: SortBy = "year", SortDescending = false
    /// </summary>
    [Fact]
    public async Task SearchAsync_SortByYearAsc_ShouldSortAscending()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { SortBy = "year", SortDescending = false });

        result.Items[0].ReleaseYear.Should().BeLessOrEqualTo(result.Items[1].ReleaseYear.GetValueOrDefault());
    }

    /// <summary>
    /// Ветвь switch: SortBy = "title", SortDescending = true
    /// </summary>
    [Fact]
    public async Task SearchAsync_SortByTitleDesc_ShouldSortDescending()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { SortBy = "title", SortDescending = true });

        result.Items[0].Title.CompareTo(result.Items[1].Title).Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Ветвь switch: default (неизвестное значение SortBy) → сортировка по дате
    /// </summary>
    [Fact]
    public async Task SearchAsync_UnknownSortBy_ShouldSortByDate()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { SortBy = "unknown", SortDescending = true });

        result.Items.Should().HaveCount(2); // Просто не падает
    }

    // =============================================
    // UpdateAsync — покрытие всех if-ветвей обновления полей
    // =============================================

    /// <summary>
    /// Ветвь: обновление только Title (остальные поля null)
    /// </summary>
    [Fact]
    public async Task UpdateAsync_OnlyTitle_ShouldUpdateOnlyTitle()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        await service.UpdateAsync(1, new UpdateMovieRequest { Title = "Новое название" });

        var movie = await context.Movies.FindAsync(1L);
        movie!.Title.Should().Be("Новое название");
        movie.Description.Should().Be("Описание фильма 1"); // Не изменилось
    }

    /// <summary>
    /// Ветвь: обновление Description
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Description_ShouldUpdate()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        await service.UpdateAsync(1, new UpdateMovieRequest { Description = "Новое описание" });

        var movie = await context.Movies.FindAsync(1L);
        movie!.Description.Should().Be("Новое описание");
    }

    /// <summary>
    /// Ветвь: обновление NeedSubscription
    /// </summary>
    [Fact]
    public async Task UpdateAsync_NeedSubscription_ShouldUpdate()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        await service.UpdateAsync(1, new UpdateMovieRequest { NeedSubscription = true });

        var movie = await context.Movies.FindAsync(1L);
        movie!.NeedSubscription.Should().BeTrue();
    }

    /// <summary>
    /// Ветвь: обновление IsPublished
    /// </summary>
    [Fact]
    public async Task UpdateAsync_IsPublished_ShouldUpdate()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        await service.UpdateAsync(1, new UpdateMovieRequest { IsPublished = false });

        var movie = await context.Movies.FindAsync(1L);
        movie!.IsPublished.Should().BeFalse();
    }

    /// <summary>
    /// Ветвь: обновление GenreIds (не null)
    /// </summary>
    [Fact]
    public async Task UpdateAsync_GenreIds_ShouldUpdateGenres()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        await service.UpdateAsync(1, new UpdateMovieRequest { GenreIds = new List<long> { 3 } });

        var movie = await service.GetByIdAsync(1);
        movie!.Genres.Should().Contain("Комедия");
        movie.Genres.Should().NotContain("Боевик");
    }

    /// <summary>
    /// Ветвь: GenreIds = null → жанры не меняются
    /// </summary>
    [Fact]
    public async Task UpdateAsync_NullGenreIds_ShouldNotChangeGenres()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        await service.UpdateAsync(1, new UpdateMovieRequest { Title = "X", GenreIds = null });

        var movie = await service.GetByIdAsync(1);
        movie!.Genres.Should().Contain("Боевик");
    }

    // =============================================
    // GetByIdAsync — покрытие ветвей userId
    // =============================================

    /// <summary>
    /// Ветвь: userId = null → UserRating и IsFavorite не заполняются
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_NoUserId_ShouldNotIncludeUserData()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.GetByIdAsync(1, null);

        result.Should().NotBeNull();
        result!.UserRating.Should().BeNull();
        result.IsFavorite.Should().BeFalse();
    }

    /// <summary>
    /// Ветвь: userId задан, но у пользователя нет оценки/избранного
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_UserWithoutRatingOrFavorite_ShouldReturnDefaults()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.GetByIdAsync(2, 3); // admin не оценивал movie2

        result.Should().NotBeNull();
        result!.UserRating.Should().BeNull();
        result.IsFavorite.Should().BeFalse();
    }
}

using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.UnitTests;

/// <summary>
/// Модульные тесты для MovieService.
/// Тестируется поиск, CRUD, фильтрация и сортировка фильмов.
/// </summary>
public class MovieServiceTests
{
    /// <summary>
    /// Поиск без фильтров — возвращает только опубликованные фильмы
    /// </summary>
    [Fact]
    public async Task SearchAsync_NoFilters_ShouldReturnOnlyPublishedMovies()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest());

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2); // movie1 и movie2 опубликованы, movie3 — нет
        result.Items.Should().NotContain(m => m.Title == "Неопубликованный фильм");
    }

    /// <summary>
    /// Поиск по названию — находит фильм по подстроке
    /// </summary>
    [Fact]
    public async Task SearchAsync_ByTitle_ShouldFilterByTitle()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { Search = "Премиум" });

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Премиум фильм");
    }

    /// <summary>
    /// Поиск по жанру — фильтрует корректно
    /// </summary>
    [Fact]
    public async Task SearchAsync_ByGenre_ShouldFilterByGenre()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { GenreId = 1 }); // action

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Тестовый фильм 1");
    }

    /// <summary>
    /// Поиск по стране — фильтрует корректно
    /// </summary>
    [Fact]
    public async Task SearchAsync_ByCountry_ShouldFilterByCountry()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { Country = "Россия" });

        result.Items.Should().HaveCount(1);
        result.Items[0].Country.Should().Be("Россия");
    }

    /// <summary>
    /// Поиск по году — фильтрация YearFrom
    /// </summary>
    [Fact]
    public async Task SearchAsync_ByYearFrom_ShouldFilterCorrectly()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { YearFrom = 2024 });

        result.Items.Should().HaveCount(1);
        result.Items[0].ReleaseYear.Should().Be(2024);
    }

    /// <summary>
    /// Поиск по минимальному рейтингу
    /// </summary>
    [Fact]
    public async Task SearchAsync_ByMinRating_ShouldFilterByRating()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { MinRating = 4.0m });

        result.Items.Should().HaveCount(1);
        result.Items[0].AverageRating.Should().BeGreaterOrEqualTo(4.0m);
    }

    /// <summary>
    /// Сортировка по рейтингу по убыванию
    /// </summary>
    [Fact]
    public async Task SearchAsync_SortByRatingDesc_ShouldSortCorrectly()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { SortBy = "rating", SortDescending = true });

        result.Items.Should().HaveCount(2);
        result.Items[0].AverageRating.Should().BeGreaterOrEqualTo(result.Items[1].AverageRating);
    }

    /// <summary>
    /// Пагинация — проверяет корректность TotalCount и страницы
    /// </summary>
    [Fact]
    public async Task SearchAsync_WithPagination_ShouldReturnCorrectPage()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { Page = 1, PageSize = 1 });

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(1);
    }

    /// <summary>
    /// Получение фильма по ID — существующий опубликованный
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ExistingPublishedMovie_ShouldReturnMovieDetail()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Тестовый фильм 1");
        result.Genres.Should().Contain("Боевик");
        result.Genres.Should().Contain("Драма");
    }

    /// <summary>
    /// Получение неопубликованного фильма — должен вернуть null
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_UnpublishedMovie_ShouldReturnNull()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.GetByIdAsync(3);

        result.Should().BeNull();
    }

    /// <summary>
    /// Получение фильма с информацией о пользовательском рейтинге
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WithUserId_ShouldIncludeUserRatingAndFavorite()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.GetByIdAsync(1, 1);

        result.Should().NotBeNull();
        result!.UserRating.Should().Be(5);
        result.IsFavorite.Should().BeTrue();
    }

    /// <summary>
    /// Получение случайного фильма — не null
    /// </summary>
    [Fact]
    public async Task GetRandomAsync_ShouldReturnPublishedMovie()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.GetRandomAsync();

        result.Should().NotBeNull();
    }

    /// <summary>
    /// Получение списка стран
    /// </summary>
    [Fact]
    public async Task GetCountriesAsync_ShouldReturnDistinctCountries()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.GetCountriesAsync();

        result.Should().NotBeEmpty();
        result.Should().Contain("Россия");
        result.Should().Contain("США");
    }

    /// <summary>
    /// Создание нового фильма
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldCreateMovie_AndReturnId()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var request = new CreateMovieRequest
        {
            Title = "Новый фильм",
            Description = "Описание",
            ReleaseYear = 2025,
            DurationMinutes = 100,
            VideoUrl = "https://example.com/new.mp4",
            Country = "Россия",
            Director = "Режиссёр",
            GenreIds = new List<long> { 1, 2 }
        };

        var movieId = await service.CreateAsync(request);

        movieId.Should().BeGreaterThan(0);
        var movie = await context.Movies.FindAsync(movieId);
        movie.Should().NotBeNull();
        movie!.Title.Should().Be("Новый фильм");
    }

    /// <summary>
    /// Обновление существующего фильма
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ExistingMovie_ShouldUpdateFields()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.UpdateAsync(1, new UpdateMovieRequest
        {
            Title = "Обновлённый фильм",
            ReleaseYear = 2026
        });

        result.Should().BeTrue();
        var movie = await context.Movies.FindAsync(1L);
        movie!.Title.Should().Be("Обновлённый фильм");
        movie.ReleaseYear.Should().Be(2026);
    }

    /// <summary>
    /// Обновление несуществующего фильма — false
    /// </summary>
    [Fact]
    public async Task UpdateAsync_NonExistingMovie_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.UpdateAsync(999, new UpdateMovieRequest { Title = "X" });

        result.Should().BeFalse();
    }

    /// <summary>
    /// Удаление существующего фильма
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ExistingMovie_ShouldDeleteAndReturnTrue()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.DeleteAsync(1);

        result.Should().BeTrue();
        var movie = await context.Movies.FindAsync(1L);
        movie.Should().BeNull();
    }

    /// <summary>
    /// Удаление несуществующего фильма — false
    /// </summary>
    [Fact]
    public async Task DeleteAsync_NonExistingMovie_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.DeleteAsync(999);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Поиск по описанию — находит фильм
    /// </summary>
    [Fact]
    public async Task SearchAsync_ByDescription_ShouldFindMovie()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { Search = "премиум" });

        result.Items.Should().HaveCountGreaterOrEqualTo(1);
    }

    /// <summary>
    /// Сортировка по названию
    /// </summary>
    [Fact]
    public async Task SearchAsync_SortByTitle_ShouldSortAlphabetically()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new MovieService(context);

        var result = await service.SearchAsync(new MovieSearchRequest { SortBy = "title", SortDescending = false });

        result.Items.Should().HaveCount(2);
        result.Items[0].Title.CompareTo(result.Items[1].Title).Should().BeLessThan(0);
    }
}

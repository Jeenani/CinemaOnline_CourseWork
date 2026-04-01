using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Tests.Helpers;

/// <summary>
/// Фабрика для создания тестового контекста БД с InMemory провайдером.
/// Используется во всех тестах для изоляции данных.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Создаёт новый CinemaOnlineContext с уникальной InMemory базой данных
    /// </summary>
    public static CinemaOnlineContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<CinemaOnlineContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new CinemaOnlineContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Создаёт контекст и заполняет его тестовыми данными
    /// </summary>
    public static CinemaOnlineContext CreateWithSeedData(string? dbName = null)
    {
        var context = Create(dbName);
        SeedData(context);
        return context;
    }

    /// <summary>
    /// Заполняет контекст стандартным набором тестовых данных
    /// </summary>
    public static void SeedData(CinemaOnlineContext context)
    {
        // Жанры
        var genreAction = new Genre { Id = 1, Name = "action", DisplayName = "Боевик" };
        var genreDrama = new Genre { Id = 2, Name = "drama", DisplayName = "Драма" };
        var genreComedy = new Genre { Id = 3, Name = "comedy", DisplayName = "Комедия" };
        context.Genres.AddRange(genreAction, genreDrama, genreComedy);

        // Подписки
        var subMonthly = new Subscription
        {
            Id = 1,
            Name = "Месячная",
            Price = 299m,
            DurationDays = 30,
            Description = "Подписка на 1 месяц",
            IsActive = true
        };
        var subYearly = new Subscription
        {
            Id = 2,
            Name = "Годовая",
            Price = 2499m,
            DurationDays = 365,
            Description = "Подписка на 1 год",
            IsActive = true
        };
        var subInactive = new Subscription
        {
            Id = 3,
            Name = "Архивная",
            Price = 99m,
            DurationDays = 7,
            Description = "Неактивная подписка",
            IsActive = false
        };
        context.Subscriptions.AddRange(subMonthly, subYearly, subInactive);

        // Фильмы
        var movie1 = new Movie
        {
            Id = 1,
            Title = "Тестовый фильм 1",
            Description = "Описание фильма 1",
            ReleaseYear = 2023,
            DurationMinutes = 120,
            VideoUrl = "https://example.com/video1.mp4",
            VkVideoUrl = "https://vk.com/video1",
            PosterUrl = "/posters/poster1.jpg",
            BannerUrl = "/banners/banner1.jpg",
            Country = "Россия",
            Director = "Режиссёр 1",
            NeedSubscription = false,
            IsPublished = true,
            AverageRating = 4.5m,
            RatingsCount = 10,
            ViewCount = 1000,
            CommentCount = 5,
            CreatedAt = DateTime.Now.AddDays(-10),
            Genres = new List<Genre> { genreAction, genreDrama }
        };

        var movie2 = new Movie
        {
            Id = 2,
            Title = "Премиум фильм",
            Description = "Описание премиум фильма",
            ReleaseYear = 2024,
            DurationMinutes = 90,
            VideoUrl = "https://example.com/video2.mp4",
            PosterUrl = "/posters/poster2.jpg",
            Country = "США",
            Director = "Режиссёр 2",
            NeedSubscription = true,
            IsPublished = true,
            AverageRating = 3.8m,
            RatingsCount = 5,
            ViewCount = 500,
            CommentCount = 2,
            CreatedAt = DateTime.Now.AddDays(-5),
            Genres = new List<Genre> { genreDrama }
        };

        var movie3 = new Movie
        {
            Id = 3,
            Title = "Неопубликованный фильм",
            Description = "Скрытый фильм",
            ReleaseYear = 2025,
            DurationMinutes = 110,
            VideoUrl = "https://example.com/video3.mp4",
            Country = "Франция",
            Director = "Режиссёр 3",
            NeedSubscription = false,
            IsPublished = false,
            CreatedAt = DateTime.Now.AddDays(-1),
            Genres = new List<Genre> { genreComedy }
        };

        context.Movies.AddRange(movie1, movie2, movie3);

        // Пользователи
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        
        var user1 = new User
        {
            Id = 1,
            Email = "user@test.com",
            PasswordHash = passwordHash,
            Name = "Тестовый пользователь",
            Role = "user",
            HasSubscription = false,
            CreatedAt = DateTime.Now.AddDays(-30)
        };

        var user2 = new User
        {
            Id = 2,
            Email = "premium@test.com",
            PasswordHash = passwordHash,
            Name = "Премиум пользователь",
            Role = "user",
            HasSubscription = true,
            SubscriptionId = 1,
            SubscriptionStartDate = DateTime.Now.AddDays(-15),
            SubscriptionEndDate = DateTime.Now.AddDays(15),
            CreatedAt = DateTime.Now.AddDays(-20)
        };

        var adminUser = new User
        {
            Id = 3,
            Email = "admin@test.com",
            PasswordHash = passwordHash,
            Name = "Администратор",
            Role = "admin",
            HasSubscription = true,
            SubscriptionId = 2,
            SubscriptionStartDate = DateTime.Now.AddDays(-100),
            SubscriptionEndDate = DateTime.Now.AddDays(265),
            CreatedAt = DateTime.Now.AddDays(-100)
        };

        context.Users.AddRange(user1, user2, adminUser);

        // Комментарии
        context.Comments.AddRange(
            new Comment { Id = 1, UserId = 1, MovieId = 1, Content = "Отличный фильм!", IsVisible = true, CreatedAt = DateTime.Now.AddDays(-5) },
            new Comment { Id = 2, UserId = 2, MovieId = 1, Content = "Мне понравилось", IsVisible = true, CreatedAt = DateTime.Now.AddDays(-3) },
            new Comment { Id = 3, UserId = 1, MovieId = 2, Content = "Скрытый комментарий", IsVisible = false, CreatedAt = DateTime.Now.AddDays(-2) }
        );

        // Рейтинги
        context.Ratings.AddRange(
            new Rating { UserId = 1, MovieId = 1, Rating1 = 5 },
            new Rating { UserId = 2, MovieId = 1, Rating1 = 4 }
        );

        // Избранное
        context.Favorites.Add(new Favorite { UserId = 1, MovieId = 1, CreatedAt = DateTime.Now.AddDays(-7) });

        // История просмотров
        context.ViewHistories.Add(new ViewHistory
        {
            Id = 1,
            UserId = 1,
            MovieId = 1,
            ProgressSeconds = 3600,
            Completed = false,
            CreatedAt = DateTime.Now.AddDays(-2),
            UpdatedAt = DateTime.Now.AddDays(-1)
        });

        // Коллекции
        var collection = new Collection
        {
            Id = 1,
            Name = "Лучшее за неделю",
            Description = "Подборка лучших фильмов",
            IsFeatured = true,
            DisplayOrder = 1,
            CreatedAt = DateTime.Now.AddDays(-7)
        };
        context.Collections.Add(collection);
        context.CollectionMovies.Add(new CollectionMovie { CollectionId = 1, MovieId = 1, Position = 1 });

        // Платежи
        context.Payments.Add(new Payment
        {
            Id = 1,
            UserId = 2,
            SubscriptionId = 1,
            Amount = 299m,
            Status = "paid",
            PaymentMethod = "card",
            TransactionId = "txn_test_001",
            CreatedAt = DateTime.Now.AddDays(-15)
        });

        context.SaveChanges();
    }
}

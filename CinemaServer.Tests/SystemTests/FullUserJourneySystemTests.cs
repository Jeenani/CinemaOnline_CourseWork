using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.SystemTests;

/// <summary>
/// Системные тесты — полные пользовательские сценарии «от начала до конца».
/// Искусственно создаются все ситуации: регистрация, просмотр, покупка, администрирование.
/// </summary>
public class FullUserJourneySystemTests
{
    /// <summary>
    /// Полный путь нового пользователя:
    /// Регистрация → Просмотр каталога → Поиск фильма → Просмотр деталей → 
    /// Оценка → Комментарий → Добавление в избранное → Покупка подписки → 
    /// Доступ к премиум-контенту
    /// </summary>
    [Fact]
    public async Task FullNewUserJourney_FromRegistrationToSubscription()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var movieService = new MovieService(context);
        var subService = new SubscriptionService(context);
        var paymentService = new PaymentService(context, userService, subService);
        var ratingService = new RatingService(context);
        var commentService = new CommentService(context);
        var favoriteService = new FavoriteService(context);
        var viewHistoryService = new ViewHistoryService(context);

        var authController = new AuthController(userService);
        var moviesController = new MoviesController(
            movieService, ratingService, commentService,
            favoriteService, viewHistoryService, userService
        );
        var subsController = new SubscriptionsController(subService);
        var payController = new PaymentsController(paymentService);
        var userController = new UserController(favoriteService, viewHistoryService, userService);

        // ========== ШАГ 1: Регистрация ==========
        var registerResult = await authController.Register(new RegisterRequest
        {
            Email = "journey@test.com",
            Password = "JourneyPass123",
            Name = "Путешественник"
        });
        var authOk = registerResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var authData = authOk.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        authData.Success.Should().BeTrue();
        var token = authData.Data!.Token;
        token.Should().NotBeNullOrEmpty();

        // ========== ШАГ 2: Просмотр каталога ==========
        var catalogResult = await moviesController.Search(new MovieSearchRequest { Page = 1, PageSize = 10 });
        var catalogOk = catalogResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var catalog = catalogOk.Value.Should().BeOfType<ApiResponse<PagedResponse<MovieResponse>>>().Subject;
        catalog.Data!.Items.Should().NotBeEmpty();

        // ========== ШАГ 3: Получение случайного фильма (баннер) ==========
        var randomResult = await moviesController.GetRandom();
        var randomOk = randomResult.Result.Should().BeOfType<OkObjectResult>().Subject;

        // ========== ШАГ 4: Поиск фильма ==========
        var searchResult = await moviesController.Search(new MovieSearchRequest { Search = "Тестовый" });
        var searchOk = searchResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var searchData = searchOk.Value.Should().BeOfType<ApiResponse<PagedResponse<MovieResponse>>>().Subject;
        searchData.Data!.Items.Should().HaveCountGreaterOrEqualTo(1);

        // ========== ШАГ 5: Просмотр деталей фильма ==========
        var movieId = searchData.Data.Items.First().Id;
        var detailResult = await moviesController.GetById(movieId, token);
        var detailOk = detailResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var detail = detailOk.Value.Should().BeOfType<ApiResponse<MovieDetailResponse>>().Subject;
        detail.Data!.Title.Should().NotBeNullOrEmpty();

        // ========== ШАГ 6: Оценка фильма ==========
        var rateResult = await moviesController.Rate(movieId, new RateMovieRequest { Rating = 5 }, token);
        rateResult.Result.Should().BeOfType<OkObjectResult>();

        // ========== ШАГ 7: Комментарий ==========
        var commentResult = await moviesController.AddComment(
            movieId, new CreateCommentRequest { Content = "Потрясающий фильм! Рекомендую всем." }, token);
        commentResult.Result.Should().BeOfType<CreatedResult>();

        // ========== ШАГ 8: Добавление в избранное ==========
        var favResult = await moviesController.AddToFavorites(movieId, token);
        favResult.Result.Should().BeOfType<OkObjectResult>();

        // ========== ШАГ 9: Запись просмотра ==========
        var viewResult = await moviesController.RecordView(
            movieId, new RecordViewRequest { ProgressSeconds = 3600, Completed = true }, token);
        viewResult.Result.Should().BeOfType<OkObjectResult>();

        // ========== ШАГ 10: Получение тарифов подписки ==========
        var plansResult = await subsController.GetPlans();
        var plansOk = plansResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var plans = plansOk.Value.Should().BeOfType<ApiResponse<List<SubscriptionPlanResponse>>>().Subject;
        plans.Data.Should().NotBeEmpty();

        // ========== ШАГ 11: Покупка подписки ==========
        var payCreateResult = await payController.Create(
            new CreatePaymentRequest { SubscriptionId = plans.Data!.First().Id, PaymentMethod = "card" },
            token
        );
        var payCreated = payCreateResult.Result.Should().BeOfType<CreatedResult>().Subject;
        var payData = payCreated.Value.Should().BeOfType<ApiResponse<PaymentResponse>>().Subject;

        // ========== ШАГ 12: Подтверждение оплаты ==========
        var processResult = await payController.Process(
            payData.Data!.Id, new ProcessPaymentRequest { Success = true });
        processResult.Result.Should().BeOfType<OkObjectResult>();

        // ========== ШАГ 13: Проверка профиля с подпиской ==========
        var meResult = await authController.GetMe(token);
        var meOk = meResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var me = meOk.Value.Should().BeOfType<ApiResponse<UserResponse>>().Subject;
        me.Data!.HasSubscription.Should().BeTrue();

        // ========== ШАГ 14: Проверка избранного ==========
        var favsResult = await userController.GetFavorites(token);
        var favsOk = favsResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var favs = favsOk.Value.Should().BeOfType<ApiResponse<List<MovieResponse>>>().Subject;
        favs.Data.Should().Contain(m => m.Id == movieId);

        // ========== ШАГ 15: Проверка истории просмотров ==========
        var histResult = await userController.GetHistory(token);
        var histOk = histResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var hist = histOk.Value.Should().BeOfType<ApiResponse<List<MovieResponse>>>().Subject;
        hist.Data.Should().Contain(m => m.Id == movieId);
    }

    /// <summary>
    /// Сценарий: попытка доступа к функционалу без авторизации
    /// Все защищённые endpoint'ы должны возвращать 401
    /// </summary>
    [Fact]
    public async Task UnauthorizedAccess_AllProtectedEndpoints_ShouldReturn401()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var moviesController = new MoviesController(
            new MovieService(context), new RatingService(context),
            new CommentService(context), new FavoriteService(context),
            new ViewHistoryService(context), userService
        );
        var payController = new PaymentsController(
            new PaymentService(context, userService, new SubscriptionService(context))
        );
        var userController = new UserController(
            new FavoriteService(context), new ViewHistoryService(context), userService
        );

        // Оценка без авторизации
        var rate = await moviesController.Rate(1, new RateMovieRequest { Rating = 5 }, null);
        rate.Result.Should().BeOfType<UnauthorizedObjectResult>();

        // Комментарий без авторизации
        var comment = await moviesController.AddComment(1, new CreateCommentRequest { Content = "X" }, null);
        comment.Result.Should().BeOfType<UnauthorizedObjectResult>();

        // Избранное без авторизации
        var fav = await moviesController.AddToFavorites(1, null);
        fav.Result.Should().BeOfType<UnauthorizedObjectResult>();

        // Запись просмотра без авторизации
        var view = await moviesController.RecordView(1, new RecordViewRequest { ProgressSeconds = 100 }, null);
        view.Result.Should().BeOfType<UnauthorizedObjectResult>();

        // Платёж без авторизации
        var pay = await payController.Create(new CreatePaymentRequest { SubscriptionId = 1, PaymentMethod = "card" }, null);
        pay.Result.Should().BeOfType<UnauthorizedObjectResult>();

        // Избранное пользователя без авторизации
        var userFavs = await userController.GetFavorites(null);
        userFavs.Result.Should().BeOfType<UnauthorizedObjectResult>();

        // История без авторизации
        var userHist = await userController.GetHistory(null);
        userHist.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Сценарий: обычный пользователь пытается выполнять действия админа
    /// Все админские endpoint'ы должны возвращать 403
    /// </summary>
    [Fact]
    public async Task RegularUserAdminActions_ShouldReturn403()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var moviesController = new MoviesController(
            new MovieService(context), new RatingService(context),
            new CommentService(context), new FavoriteService(context),
            new ViewHistoryService(context), userService
        );

        var userToken = "Bearer_1_user_guid";

        // Создание фильма
        var create = await moviesController.Create(
            new CreateMovieRequest { Title = "X", VideoUrl = "x" }, userToken);
        (create.Result as ObjectResult)!.StatusCode.Should().Be(403);

        // Обновление фильма
        var update = await moviesController.Update(1, new UpdateMovieRequest { Title = "X" }, userToken);
        (update.Result as ObjectResult)!.StatusCode.Should().Be(403);

        // Удаление фильма
        var delete = await moviesController.Delete(1, userToken);
        (delete.Result as ObjectResult)!.StatusCode.Should().Be(403);
    }

    /// <summary>
    /// Сценарий: админ управляет контентом
    /// Создание фильма → Обновление → Публикация → Удаление
    /// </summary>
    [Fact]
    public async Task AdminContentManagement_FullCRUDCycle()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var movieService = new MovieService(context);
        var moviesController = new MoviesController(
            movieService, new RatingService(context),
            new CommentService(context), new FavoriteService(context),
            new ViewHistoryService(context), userService
        );

        var adminToken = "Bearer_3_admin_guid";

        // 1. Создание фильма
        var createResult = await moviesController.Create(new CreateMovieRequest
        {
            Title = "Админский фильм",
            Description = "Создан админом",
            VideoUrl = "https://example.com/admin.mp4",
            ReleaseYear = 2025,
            Country = "Россия",
            GenreIds = new List<long> { 1, 2 }
        }, adminToken);
        var created = createResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var createResponse = created.Value.Should().BeOfType<ApiResponse<long>>().Subject;
        var newMovieId = createResponse.Data;
        newMovieId.Should().BeGreaterThan(0);

        // 2. Обновление фильма
        var updateResult = await moviesController.Update(newMovieId, new UpdateMovieRequest
        {
            Title = "Обновлённый админский фильм",
            NeedSubscription = true
        }, adminToken);
        var updateOk = updateResult.Result.Should().BeOfType<OkObjectResult>().Subject;

        // 3. Проверяем обновление
        var movie = await movieService.GetByIdAsync(newMovieId);
        movie.Should().NotBeNull();
        movie!.Title.Should().Be("Обновлённый админский фильм");
        movie.NeedSubscription.Should().BeTrue();

        // 4. Удаление фильма
        var deleteResult = await moviesController.Delete(newMovieId, adminToken);
        deleteResult.Result.Should().BeOfType<OkObjectResult>();

        // 5. Проверяем удаление
        var deletedMovie = await movieService.GetByIdAsync(newMovieId);
        deletedMovie.Should().BeNull();
    }

    /// <summary>
    /// Сценарий: пользователь с истёкшей подпиской не получает доступ к премиум-контенту
    /// </summary>
    [Fact]
    public async Task ExpiredSubscription_ShouldDenyPremiumAccess()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);

        // Устанавливаем истёкшую подписку
        var user = await context.Users.FindAsync(1L);
        user!.HasSubscription = true;
        user.SubscriptionEndDate = DateTime.Now.AddDays(-1);
        await context.SaveChangesAsync();

        var hasSubscription = await userService.HasActiveSubscriptionAsync(1);
        hasSubscription.Should().BeFalse();
    }

    /// <summary>
    /// Сценарий: двойная регистрация с одним email — ошибка
    /// </summary>
    [Fact]
    public async Task DoubleRegistration_SameEmail_ShouldFail()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var authController = new AuthController(userService);

        // Первая регистрация
        var result1 = await authController.Register(new RegisterRequest
        {
            Email = "double@test.com", Password = "pass123", Name = "User1"
        });
        result1.Result.Should().BeOfType<OkObjectResult>();

        // Вторая с тем же email
        var result2 = await authController.Register(new RegisterRequest
        {
            Email = "double@test.com", Password = "pass456", Name = "User2"
        });
        result2.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Сценарий: конкурентные действия — два пользователя оценивают один фильм
    /// </summary>
    [Fact]
    public async Task ConcurrentRatings_DifferentUsers_ShouldBothSucceed()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var ratingService = new RatingService(context);

        // Пользователь 1 оценивает movie2
        var result1 = await ratingService.RateMovieAsync(1, 2, 5);
        result1.Should().BeTrue();

        // Пользователь 2 оценивает movie2
        var result2 = await ratingService.RateMovieAsync(2, 2, 3);
        result2.Should().BeTrue();

        // Обе оценки сохранены
        var rating1 = await ratingService.GetUserRatingAsync(1, 2);
        rating1.Should().Be(5);
        var rating2 = await ratingService.GetUserRatingAsync(2, 2);
        rating2.Should().Be(3);
    }

    /// <summary>
    /// Сценарий: фильтрация по нескольким параметрам одновременно
    /// </summary>
    [Fact]
    public async Task ComplexSearch_MultipleFilters_ShouldWorkCorrectly()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var movieService = new MovieService(context);

        var result = await movieService.SearchAsync(new MovieSearchRequest
        {
            Country = "Россия",
            YearFrom = 2020,
            YearTo = 2025,
            MinRating = 4.0m,
            SortBy = "rating",
            SortDescending = true
        });

        result.Items.Should().OnlyContain(m =>
            m.Country == "Россия" &&
            m.ReleaseYear >= 2020 &&
            m.ReleaseYear <= 2025 &&
            m.AverageRating >= 4.0m
        );
    }
}

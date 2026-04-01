using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.BlackBoxTests;

/// <summary>
/// Тесты чёрного ящика для Subscriptions и Payments API.
/// Тестируется получение тарифов, создание платежей — поведение с точки зрения пользователя.
/// </summary>
public class SubscriptionsApiBlackBoxTests
{
    // =============================================
    // GET /api/subscriptions — тарифы
    // =============================================

    /// <summary>
    /// Получение списка тарифов → 200 OK, непустой список
    /// </summary>
    [Fact]
    public async Task GetPlans_ShouldReturn200WithPlans()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new SubscriptionsController(new SubscriptionService(context));

        var result = await controller.GetPlans();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<List<SubscriptionPlanResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeEmpty();
    }

    /// <summary>
    /// Получение тарифа по ID → 200 OK
    /// </summary>
    [Fact]
    public async Task GetPlan_ExistingId_ShouldReturn200()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new SubscriptionsController(new SubscriptionService(context));

        var result = await controller.GetPlan(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<SubscriptionPlanResponse>>().Subject;
        response.Data!.Name.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Получение несуществующего тарифа → 404 Not Found
    /// </summary>
    [Fact]
    public async Task GetPlan_NonExistingId_ShouldReturn404()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new SubscriptionsController(new SubscriptionService(context));

        var result = await controller.GetPlan(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =============================================
    // POST /api/payments — платежи
    // =============================================

    /// <summary>
    /// Создание платежа авторизованным пользователем → 201 Created
    /// </summary>
    [Fact]
    public async Task CreatePayment_AuthorizedUser_ShouldReturn201()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new PaymentsController(
            new PaymentService(context, new UserService(context), new SubscriptionService(context))
        );

        var result = await controller.Create(
            new CreatePaymentRequest { SubscriptionId = 1, PaymentMethod = "card" },
            "Bearer_1_user_guid"
        );

        var createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    /// <summary>
    /// Создание платежа без авторизации → 401
    /// </summary>
    [Fact]
    public async Task CreatePayment_NoAuth_ShouldReturn401()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new PaymentsController(
            new PaymentService(context, new UserService(context), new SubscriptionService(context))
        );

        var result = await controller.Create(
            new CreatePaymentRequest { SubscriptionId = 1, PaymentMethod = "card" },
            null
        );

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    /// <summary>
    /// Создание платежа с несуществующей подпиской → 400
    /// </summary>
    [Fact]
    public async Task CreatePayment_InvalidSubscription_ShouldReturn400()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new PaymentsController(
            new PaymentService(context, new UserService(context), new SubscriptionService(context))
        );

        var result = await controller.Create(
            new CreatePaymentRequest { SubscriptionId = 999, PaymentMethod = "card" },
            "Bearer_1_user_guid"
        );

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Получение истории платежей → 200 OK
    /// </summary>
    [Fact]
    public async Task GetMyPayments_Authorized_ShouldReturn200()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var controller = new PaymentsController(
            new PaymentService(context, new UserService(context), new SubscriptionService(context))
        );

        var result = await controller.GetMyPayments("Bearer_2_user_guid");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }
}

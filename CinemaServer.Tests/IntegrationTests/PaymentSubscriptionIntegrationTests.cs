using CinemaServer.Controllers;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CinemaServer.Tests.IntegrationTests;

/// <summary>
/// Интеграционные тесты: взаимодействие модулей платежей и подписок.
/// Проверяется полный цикл: выбор тарифа → создание платежа → обработка → активация подписки.
/// </summary>
public class PaymentSubscriptionIntegrationTests
{
    /// <summary>
    /// Полный цикл покупки подписки: тариф → платёж → обработка → проверка подписки
    /// </summary>
    [Fact]
    public async Task FullSubscriptionPurchaseFlow_ShouldActivateSubscription()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var paymentService = new PaymentService(context, userService, subService);
        var subsController = new SubscriptionsController(subService);
        var payController = new PaymentsController(paymentService);
        var userController = new UserController(
            new FavoriteService(context),
            new ViewHistoryService(context),
            userService
        );

        var token = "Bearer_1_user_guid";

        // 1. Проверяем, что подписки нет
        var subBefore = await userController.GetSubscription(token);
        var subBeforeOk = subBefore.Result.Should().BeOfType<OkObjectResult>().Subject;
        var subBeforeData = subBeforeOk.Value.Should().BeOfType<ApiResponse<SubscriptionInfo>>().Subject;
        subBeforeData.Data.Should().BeNull(); // Нет подписки

        // 2. Получаем список тарифов
        var plansResult = await subsController.GetPlans();
        var plansOk = plansResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var plans = plansOk.Value.Should().BeOfType<ApiResponse<List<SubscriptionPlanResponse>>>().Subject;
        var selectedPlan = plans.Data!.First();

        // 3. Создаём платёж
        var payResult = await payController.Create(
            new CreatePaymentRequest { SubscriptionId = selectedPlan.Id, PaymentMethod = "card" },
            token
        );
        var payCreated = payResult.Result.Should().BeOfType<CreatedResult>().Subject;
        var payResponse = payCreated.Value.Should().BeOfType<ApiResponse<PaymentResponse>>().Subject;
        payResponse.Data!.Status.Should().Be("pending");

        // 4. Обрабатываем платёж (успех)
        var processResult = await payController.Process(payResponse.Data.Id, new ProcessPaymentRequest { Success = true });
        var processOk = processResult.Result.Should().BeOfType<OkObjectResult>().Subject;

        // 5. Проверяем, что подписка активирована
        var hasSubscription = await userService.HasActiveSubscriptionAsync(1);
        hasSubscription.Should().BeTrue();
    }

    /// <summary>
    /// Неуспешная оплата не активирует подписку
    /// </summary>
    [Fact]
    public async Task FailedPayment_ShouldNotActivateSubscription()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var paymentService = new PaymentService(context, userService, subService);
        var payController = new PaymentsController(paymentService);

        var token = "Bearer_1_user_guid";

        // 1. Создаём платёж
        var payResult = await payController.Create(
            new CreatePaymentRequest { SubscriptionId = 1, PaymentMethod = "card" },
            token
        );
        var payCreated = payResult.Result.Should().BeOfType<CreatedResult>().Subject;
        var payResponse = payCreated.Value.Should().BeOfType<ApiResponse<PaymentResponse>>().Subject;

        // 2. Обрабатываем — неуспех
        await payController.Process(payResponse.Data!.Id, new ProcessPaymentRequest { Success = false });

        // 3. Подписка НЕ активирована
        var hasSubscription = await userService.HasActiveSubscriptionAsync(1);
        hasSubscription.Should().BeFalse();
    }

    /// <summary>
    /// История платежей содержит все созданные платежи
    /// </summary>
    [Fact]
    public async Task PaymentHistory_ShouldContainAllPayments()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var paymentService = new PaymentService(context, userService, subService);
        var payController = new PaymentsController(paymentService);

        var token = "Bearer_1_user_guid";

        // Создаём 2 платежа
        await payController.Create(
            new CreatePaymentRequest { SubscriptionId = 1, PaymentMethod = "card" }, token);
        await payController.Create(
            new CreatePaymentRequest { SubscriptionId = 2, PaymentMethod = "paypal" }, token);

        // Получаем историю
        var historyResult = await payController.GetMyPayments(token);
        var historyOk = historyResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var history = historyOk.Value.Should().BeOfType<ApiResponse<List<PaymentResponse>>>().Subject;
        history.Data.Should().HaveCount(2);
    }
}

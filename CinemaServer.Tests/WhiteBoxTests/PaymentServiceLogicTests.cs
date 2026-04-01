using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Tests.WhiteBoxTests;

/// <summary>
/// Тесты белого ящика для PaymentService.
/// Проверяется логика обработки платежей: все ветви success/fail, повторная обработка.
/// </summary>
public class PaymentServiceLogicTests
{
    /// <summary>
    /// Ветвь ProcessAsync: success=true → статус "paid", подписка активирована, история создана
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Success_ShouldSetPaidStatus_ActivateSubscription_CreateHistory()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        var payment = await service.CreateAsync(1, 1, "card");
        var result = await service.ProcessAsync(payment.Id, true);

        result.Should().BeTrue();

        // Проверяем статус платежа
        var dbPayment = await context.Payments.FindAsync(payment.Id);
        dbPayment!.Status.Should().Be("paid");

        // Проверяем подписку пользователя
        var user = await context.Users.FindAsync(1L);
        user!.HasSubscription.Should().BeTrue();
        user.SubscriptionEndDate.Should().BeAfter(DateTime.Now);

        // Проверяем историю подписок
        var history = await context.SubscriptionHistories
            .Where(h => h.UserId == 1)
            .FirstOrDefaultAsync();
        history.Should().NotBeNull();
        history!.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Ветвь ProcessAsync: success=false → статус "failed", подписка НЕ активирована
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Failed_ShouldSetFailedStatus_NotActivateSubscription()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        var payment = await service.CreateAsync(1, 1, "card");
        var result = await service.ProcessAsync(payment.Id, false);

        result.Should().BeTrue();

        var dbPayment = await context.Payments.FindAsync(payment.Id);
        dbPayment!.Status.Should().Be("failed");

        var user = await context.Users.FindAsync(1L);
        user!.HasSubscription.Should().BeFalse();
    }

    /// <summary>
    /// Ветвь ProcessAsync: платёж уже обработан (status != "pending") → return false
    /// </summary>
    [Fact]
    public async Task ProcessAsync_AlreadyProcessed_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        // Платёж id=1 уже "paid"
        var result = await service.ProcessAsync(1, true);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Ветвь ProcessAsync: payment == null → return false
    /// </summary>
    [Fact]
    public async Task ProcessAsync_NonExisting_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        var result = await service.ProcessAsync(999, true);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Ветвь CreateAsync: plan == null → throw ArgumentException
    /// </summary>
    [Fact]
    public async Task CreateAsync_InvalidPlan_ShouldThrow()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        var act = () => service.CreateAsync(1, 999, "card");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not found*");
    }

    /// <summary>
    /// Проверка корректности TransactionId — формат txn_{guid}
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldGenerateUniqueTransactionId()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        var payment1 = await service.CreateAsync(1, 1, "card");
        var payment2 = await service.CreateAsync(1, 2, "paypal");

        payment1.TransactionId.Should().StartWith("txn_");
        payment2.TransactionId.Should().StartWith("txn_");
        payment1.TransactionId.Should().NotBe(payment2.TransactionId);
    }
}

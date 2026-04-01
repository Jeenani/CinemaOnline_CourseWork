using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.UnitTests;

/// <summary>
/// Модульные тесты для PaymentService.
/// Тестируется создание, обработка платежей и получение истории.
/// </summary>
public class PaymentServiceTests
{
    /// <summary>
    /// Создание платежа — успех
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidData_ShouldCreatePayment()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        var result = await service.CreateAsync(1, 1, "card");

        result.Should().NotBeNull();
        result.Status.Should().Be("pending");
        result.Amount.Should().Be(299m);
        result.TransactionId.Should().StartWith("txn_");
    }

    /// <summary>
    /// Создание платежа с несуществующей подпиской — исключение
    /// </summary>
    [Fact]
    public async Task CreateAsync_InvalidSubscription_ShouldThrowArgumentException()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        var act = () => service.CreateAsync(1, 999, "card");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Обработка платежа — успешная оплата
    /// </summary>
    [Fact]
    public async Task ProcessAsync_SuccessfulPayment_ShouldActivateSubscription()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        // Создаём новый платёж
        var payment = await service.CreateAsync(1, 1, "card");

        // Обрабатываем
        var result = await service.ProcessAsync(payment.Id, true);

        result.Should().BeTrue();

        // Проверяем, что подписка активирована
        var hasSubscription = await userService.HasActiveSubscriptionAsync(1);
        hasSubscription.Should().BeTrue();
    }

    /// <summary>
    /// Обработка платежа — неуспешная оплата
    /// </summary>
    [Fact]
    public async Task ProcessAsync_FailedPayment_ShouldNotActivateSubscription()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        var payment = await service.CreateAsync(1, 1, "card");
        var result = await service.ProcessAsync(payment.Id, false);

        result.Should().BeTrue();
        var hasSubscription = await userService.HasActiveSubscriptionAsync(1);
        hasSubscription.Should().BeFalse();
    }

    /// <summary>
    /// Обработка несуществующего платежа — false
    /// </summary>
    [Fact]
    public async Task ProcessAsync_NonExistingPayment_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        var result = await service.ProcessAsync(999, true);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Повторная обработка уже обработанного платежа — false
    /// </summary>
    [Fact]
    public async Task ProcessAsync_AlreadyProcessedPayment_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        // Платёж с Id=1 уже в статусе "paid"
        var result = await service.ProcessAsync(1, true);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Получение истории платежей пользователя
    /// </summary>
    [Fact]
    public async Task GetUserPaymentsAsync_ShouldReturnUserPayments()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        var payments = await service.GetUserPaymentsAsync(2);

        payments.Should().HaveCount(1);
        payments[0].Status.Should().Be("paid");
        payments[0].Amount.Should().Be(299m);
    }

    /// <summary>
    /// Получение истории платежей пользователя без платежей — пустой список
    /// </summary>
    [Fact]
    public async Task GetUserPaymentsAsync_NoPayments_ShouldReturnEmpty()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var userService = new UserService(context);
        var subService = new SubscriptionService(context);
        var service = new PaymentService(context, userService, subService);

        var payments = await service.GetUserPaymentsAsync(1);

        payments.Should().BeEmpty();
    }
}

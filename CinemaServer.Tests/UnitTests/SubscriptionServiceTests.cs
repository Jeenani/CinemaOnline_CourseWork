using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.UnitTests;

/// <summary>
/// Модульные тесты для SubscriptionService.
/// Тестируется получение тарифов подписки.
/// </summary>
public class SubscriptionServiceTests
{
    /// <summary>
    /// Получение всех активных тарифов — только активные
    /// </summary>
    [Fact]
    public async Task GetAllPlansAsync_ShouldReturnOnlyActivePlans()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new SubscriptionService(context);

        var plans = await service.GetAllPlansAsync();

        plans.Should().HaveCount(2); // Месячная и Годовая, не Архивная
        plans.Should().OnlyContain(p => p.IsActive);
    }

    /// <summary>
    /// Тарифы отсортированы по цене
    /// </summary>
    [Fact]
    public async Task GetAllPlansAsync_ShouldBeSortedByPrice()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new SubscriptionService(context);

        var plans = await service.GetAllPlansAsync();

        plans[0].Price.Should().BeLessThan(plans[1].Price);
    }

    /// <summary>
    /// Получение тарифа по ID — существующий
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ExistingPlan_ShouldReturnPlan()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new SubscriptionService(context);

        var plan = await service.GetByIdAsync(1);

        plan.Should().NotBeNull();
        plan!.Name.Should().Be("Месячная");
        plan.Price.Should().Be(299m);
        plan.DurationDays.Should().Be(30);
    }

    /// <summary>
    /// Получение тарифа по ID — несуществующий
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_NonExistingPlan_ShouldReturnNull()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new SubscriptionService(context);

        var plan = await service.GetByIdAsync(999);

        plan.Should().BeNull();
    }

    /// <summary>
    /// Получение неактивного тарифа по ID — должен вернуть его
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_InactivePlan_ShouldStillReturn()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new SubscriptionService(context);

        var plan = await service.GetByIdAsync(3);

        plan.Should().NotBeNull();
        plan!.IsActive.Should().BeFalse();
    }
}

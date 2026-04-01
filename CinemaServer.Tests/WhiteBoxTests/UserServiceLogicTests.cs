using CinemaServer.Models;
using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.WhiteBoxTests;

/// <summary>
/// Тесты белого ящика для UserService.
/// Проверяется маппинг данных, граничные условия подписки.
/// </summary>
public class UserServiceLogicTests
{
    /// <summary>
    /// MapToResponse: пользователь без подписки → Subscription = null, HasSubscription = false
    /// </summary>
    [Fact]
    public async Task MapToResponse_UserWithoutSubscription_ShouldMapCorrectly()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.HasSubscription.Should().BeFalse();
        result.Subscription.Should().BeNull();
    }

    /// <summary>
    /// MapToResponse: пользователь с подпиской → Subscription заполнен
    /// </summary>
    [Fact]
    public async Task MapToResponse_UserWithSubscription_ShouldIncludeSubscriptionInfo()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var result = await service.GetByIdAsync(2);

        result.Should().NotBeNull();
        result!.HasSubscription.Should().BeTrue();
        result.Subscription.Should().NotBeNull();
        result.Subscription!.Id.Should().Be(1);
        result.Subscription.StartDate.Should().NotBeNull();
        result.Subscription.EndDate.Should().NotBeNull();
    }

    /// <summary>
    /// HasActiveSubscriptionAsync: подписка истекла → false
    /// </summary>
    [Fact]
    public async Task HasActiveSubscriptionAsync_ExpiredSubscription_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.Create();
        context.Users.Add(new User
        {
            Id = 10,
            Email = "expired@test.com",
            PasswordHash = "hash",
            Name = "Expired User",
            Role = "user",
            HasSubscription = true,
            SubscriptionEndDate = DateTime.Now.AddDays(-1) // Истекла вчера
        });
        context.SaveChanges();

        var service = new UserService(context);
        var result = await service.HasActiveSubscriptionAsync(10);

        result.Should().BeFalse();
    }

    /// <summary>
    /// HasActiveSubscriptionAsync: HasSubscription=false, но дата в будущем → false
    /// </summary>
    [Fact]
    public async Task HasActiveSubscriptionAsync_FlagFalseButDateFuture_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.Create();
        context.Users.Add(new User
        {
            Id = 11,
            Email = "flagfalse@test.com",
            PasswordHash = "hash",
            Name = "Flag False User",
            Role = "user",
            HasSubscription = false,
            SubscriptionEndDate = DateTime.Now.AddDays(30)
        });
        context.SaveChanges();

        var service = new UserService(context);
        var result = await service.HasActiveSubscriptionAsync(11);

        result.Should().BeFalse();
    }

    /// <summary>
    /// HasActiveSubscriptionAsync: несуществующий пользователь → false
    /// </summary>
    [Fact]
    public async Task HasActiveSubscriptionAsync_NonExistingUser_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var result = await service.HasActiveSubscriptionAsync(999);

        result.Should().BeFalse();
    }

    /// <summary>
    /// UpdateSubscriptionAsync: проверяет корректность установки дат
    /// </summary>
    [Fact]
    public async Task UpdateSubscriptionAsync_ShouldSetCorrectDates()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var beforeUpdate = DateTime.Now;
        await service.UpdateSubscriptionAsync(1, 1, 30);
        var afterUpdate = DateTime.Now;

        var user = await context.Users.FindAsync(1L);
        user!.SubscriptionStartDate.Should().BeOnOrAfter(beforeUpdate);
        user.SubscriptionStartDate.Should().BeOnOrBefore(afterUpdate);
        user.SubscriptionEndDate.Should().BeOnOrAfter(beforeUpdate.AddDays(30));
    }

    /// <summary>
    /// CreateAsync: проверяет, что роль по умолчанию "user"
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldSetDefaultRole()
    {
        var context = TestDbContextFactory.Create();
        var service = new UserService(context);

        var userId = await service.CreateAsync("test@test.com", "hash", "Test");

        var user = await context.Users.FindAsync(userId);
        user!.Role.Should().Be("user");
    }

    /// <summary>
    /// CreateAsync: проверяет, что CreatedAt и UpdatedAt заполнены
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldSetTimestamps()
    {
        var context = TestDbContextFactory.Create();
        var service = new UserService(context);

        var before = DateTime.Now;
        var userId = await service.CreateAsync("test@test.com", "hash", "Test");

        var user = await context.Users.FindAsync(userId);
        user!.CreatedAt.Should().BeOnOrAfter(before);
        user.UpdatedAt.Should().BeOnOrAfter(before);
    }
}

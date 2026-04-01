using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.UnitTests;

/// <summary>
/// Расширенные модульные тесты для UserService.
/// Дополнительное покрытие: обновление профиля, смена пароля, деактивация подписки.
/// </summary>
public class UserServiceExtendedTests
{
    /// <summary>
    /// Обновление профиля — имя и email обновились
    /// </summary>
    [Fact]
    public async Task UpdateProfileAsync_ValidData_ShouldUpdateNameAndEmail()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var result = await service.UpdateProfileAsync(1, "Новое Имя", "newemail@test.com");

        result.Should().BeTrue();
        var user = await service.GetByIdAsync(1);
        user!.Name.Should().Be("Новое Имя");
        user.Email.Should().Be("newemail@test.com");
    }

    /// <summary>
    /// Обновление профиля с email другого пользователя — отказ
    /// </summary>
    [Fact]
    public async Task UpdateProfileAsync_DuplicateEmail_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var result = await service.UpdateProfileAsync(1, "Имя", "premium@test.com");

        result.Should().BeFalse();
    }

    /// <summary>
    /// Обновление профиля несуществующего пользователя — отказ
    /// </summary>
    [Fact]
    public async Task UpdateProfileAsync_NonExistingUser_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var result = await service.UpdateProfileAsync(999, "Имя", "test@test.com");

        result.Should().BeFalse();
    }

    /// <summary>
    /// Обновление профиля — email остаётся тот же (не должен считаться дубликатом)
    /// </summary>
    [Fact]
    public async Task UpdateProfileAsync_SameEmail_ShouldSucceed()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var result = await service.UpdateProfileAsync(1, "Новое Имя", "user@test.com");

        result.Should().BeTrue();
    }

    /// <summary>
    /// Смена пароля — новый хеш сохраняется
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_ShouldUpdatePasswordHash()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var newHash = BCrypt.Net.BCrypt.HashPassword("newPassword");
        var result = await service.ChangePasswordAsync(1, newHash);

        result.Should().BeTrue();
        var hash = await service.GetPasswordHashByIdAsync(1);
        BCrypt.Net.BCrypt.Verify("newPassword", hash).Should().BeTrue();
    }

    /// <summary>
    /// Смена пароля — несуществующий пользователь
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_NonExistingUser_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var result = await service.ChangePasswordAsync(999, "hash");

        result.Should().BeFalse();
    }

    /// <summary>
    /// Получение хеша пароля по Id
    /// </summary>
    [Fact]
    public async Task GetPasswordHashByIdAsync_ExistingUser_ShouldReturnHash()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var hash = await service.GetPasswordHashByIdAsync(1);

        hash.Should().NotBeNullOrEmpty();
        BCrypt.Net.BCrypt.Verify("password123", hash).Should().BeTrue();
    }

    /// <summary>
    /// Получение хеша пароля для несуществующего пользователя
    /// </summary>
    [Fact]
    public async Task GetPasswordHashByIdAsync_NonExisting_ShouldReturnNull()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var hash = await service.GetPasswordHashByIdAsync(999);

        hash.Should().BeNull();
    }

    /// <summary>
    /// HasPremiumSubscriptionAsync — у пользователя без подписки
    /// </summary>
    [Fact]
    public async Task HasPremiumSubscriptionAsync_UserWithoutSubscription_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var result = await service.HasPremiumSubscriptionAsync(1);

        result.Should().BeFalse();
    }

    /// <summary>
    /// HasPremiumSubscriptionAsync — несуществующий пользователь
    /// </summary>
    [Fact]
    public async Task HasPremiumSubscriptionAsync_NonExistingUser_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var result = await service.HasPremiumSubscriptionAsync(999);

        result.Should().BeFalse();
    }

    /// <summary>
    /// DeactivateExpiredSubscriptionsAsync — деактивирует просроченные подписки
    /// </summary>
    [Fact]
    public async Task DeactivateExpiredSubscriptionsAsync_ShouldDeactivateExpired()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Устанавливаем просроченную подписку
        var user = await context.Users.FindAsync(1L);
        user!.HasSubscription = true;
        user.SubscriptionId = 1;
        user.SubscriptionStartDate = System.DateTime.Now.AddDays(-40);
        user.SubscriptionEndDate = System.DateTime.Now.AddDays(-10);
        await context.SaveChangesAsync();

        var count = await service.DeactivateExpiredSubscriptionsAsync();

        count.Should().BeGreaterOrEqualTo(1);
        var userAfter = await service.GetByIdAsync(1);
        userAfter!.HasSubscription.Should().BeFalse();
    }

    /// <summary>
    /// HasActiveSubscriptionAsync — просроченная подписка автоматически деактивируется
    /// </summary>
    [Fact]
    public async Task HasActiveSubscriptionAsync_ExpiredSubscription_ShouldAutoDeactivate()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Устанавливаем просроченную подписку
        var user = await context.Users.FindAsync(1L);
        user!.HasSubscription = true;
        user.SubscriptionId = 1;
        user.SubscriptionEndDate = System.DateTime.Now.AddDays(-1);
        await context.SaveChangesAsync();

        var result = await service.HasActiveSubscriptionAsync(1);

        result.Should().BeFalse();
    }

    /// <summary>
    /// HasActiveSubscriptionAsync — несуществующий пользователь
    /// </summary>
    [Fact]
    public async Task HasActiveSubscriptionAsync_NonExistingUser_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        var result = await service.HasActiveSubscriptionAsync(999);

        result.Should().BeFalse();
    }
}

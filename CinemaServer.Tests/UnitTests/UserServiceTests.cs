using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.UnitTests;

/// <summary>
/// Модульные тесты для UserService.
/// Тестируется каждый метод сервиса изолированно с InMemory БД.
/// </summary>
public class UserServiceTests
{
    /// <summary>
    /// Проверяет создание нового пользователя и возврат корректного Id
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldCreateUser_AndReturnId()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new UserService(context);

        // Act
        var userId = await service.CreateAsync("newuser@test.com", "hashedpwd", "Новый пользователь");

        // Assert
        userId.Should().BeGreaterThan(0);
        var user = await context.Users.FindAsync(userId);
        user.Should().NotBeNull();
        user!.Email.Should().Be("newuser@test.com");
        user.Name.Should().Be("Новый пользователь");
        user.Role.Should().Be("user");
    }

    /// <summary>
    /// Проверяет получение пользователя по ID (существующий пользователь)
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ExistingUser_ShouldReturnUserResponse()
    {
        // Arrange
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("user@test.com");
        result.Name.Should().Be("Тестовый пользователь");
        result.Role.Should().Be("user");
        result.HasSubscription.Should().BeFalse();
    }

    /// <summary>
    /// Проверяет возврат null при запросе несуществующего пользователя
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ShouldReturnNull()
    {
        // Arrange
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Проверяет поиск пользователя по email (существующий)
    /// </summary>
    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Act
        var result = await service.GetByEmailAsync("user@test.com");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Email.Should().Be("user@test.com");
    }

    /// <summary>
    /// Проверяет возврат null при поиске по несуществующему email
    /// </summary>
    [Fact]
    public async Task GetByEmailAsync_NonExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Act
        var result = await service.GetByEmailAsync("nonexistent@test.com");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Проверяет получение хеша пароля по email
    /// </summary>
    [Fact]
    public async Task GetPasswordHashAsync_ExistingEmail_ShouldReturnHash()
    {
        // Arrange
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Act
        var hash = await service.GetPasswordHashAsync("user@test.com");

        // Assert
        hash.Should().NotBeNullOrEmpty();
        BCrypt.Net.BCrypt.Verify("password123", hash).Should().BeTrue();
    }

    /// <summary>
    /// Проверяет возврат null для хеша пароля несуществующего пользователя
    /// </summary>
    [Fact]
    public async Task GetPasswordHashAsync_NonExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Act
        var hash = await service.GetPasswordHashAsync("nonexistent@test.com");

        // Assert
        hash.Should().BeNull();
    }

    /// <summary>
    /// Проверяет обновление подписки пользователя
    /// </summary>
    [Fact]
    public async Task UpdateSubscriptionAsync_ExistingUser_ShouldUpdateSubscription()
    {
        // Arrange
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Act
        var result = await service.UpdateSubscriptionAsync(1, 1, 30);

        // Assert
        result.Should().BeTrue();
        var user = await context.Users.FindAsync(1L);
        user!.HasSubscription.Should().BeTrue();
        user.SubscriptionId.Should().Be(1);
        user.SubscriptionEndDate.Should().BeAfter(DateTime.Now);
    }

    /// <summary>
    /// Проверяет обновление подписки для несуществующего пользователя
    /// </summary>
    [Fact]
    public async Task UpdateSubscriptionAsync_NonExistingUser_ShouldReturnFalse()
    {
        // Arrange
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Act
        var result = await service.UpdateSubscriptionAsync(999, 1, 30);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Проверяет наличие активной подписки у пользователя с подпиской
    /// </summary>
    [Fact]
    public async Task HasActiveSubscriptionAsync_UserWithSubscription_ShouldReturnTrue()
    {
        // Arrange
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Act
        var result = await service.HasActiveSubscriptionAsync(2);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Проверяет отсутствие активной подписки у обычного пользователя
    /// </summary>
    [Fact]
    public async Task HasActiveSubscriptionAsync_UserWithoutSubscription_ShouldReturnFalse()
    {
        // Arrange
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Act
        var result = await service.HasActiveSubscriptionAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Проверяет данные подписки у премиум-пользователя
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_PremiumUser_ShouldIncludeSubscriptionInfo()
    {
        // Arrange
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new UserService(context);

        // Act
        var result = await service.GetByIdAsync(2);

        // Assert
        result.Should().NotBeNull();
        result!.HasSubscription.Should().BeTrue();
        result.Subscription.Should().NotBeNull();
        result.Subscription!.Name.Should().Be("Месячная");
    }
}

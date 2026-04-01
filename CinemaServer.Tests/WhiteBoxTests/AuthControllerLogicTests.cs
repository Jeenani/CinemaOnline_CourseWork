using CinemaServer.Controllers;
using FluentAssertions;

namespace CinemaServer.Tests.WhiteBoxTests;

/// <summary>
/// Тесты белого ящика для AuthController.
/// Проверяется внутренняя логика генерации и парсинга токенов.
/// Тестируются все ветви условий (branch coverage).
/// </summary>
public class AuthControllerLogicTests
{
    // =============================================
    // GetUserIdFromToken — анализ всех ветвей
    // =============================================

    /// <summary>
    /// Ветвь 1: authorization = null → возвращает null
    /// </summary>
    [Fact]
    public void GetUserIdFromToken_NullInput_ShouldReturnNull()
    {
        var result = AuthController.GetUserIdFromToken(null);
        result.Should().BeNull();
    }

    /// <summary>
    /// Ветвь 2: authorization = "" → возвращает null
    /// </summary>
    [Fact]
    public void GetUserIdFromToken_EmptyString_ShouldReturnNull()
    {
        var result = AuthController.GetUserIdFromToken("");
        result.Should().BeNull();
    }

    /// <summary>
    /// Ветвь 3: некорректный формат токена (нет разделителей) → null
    /// </summary>
    [Fact]
    public void GetUserIdFromToken_InvalidFormat_ShouldReturnNull()
    {
        var result = AuthController.GetUserIdFromToken("invalidtoken");
        result.Should().BeNull();
    }

    /// <summary>
    /// Ветвь 4: формат с одним разделителем, но userId не число → null
    /// </summary>
    [Fact]
    public void GetUserIdFromToken_NonNumericUserId_ShouldReturnNull()
    {
        var result = AuthController.GetUserIdFromToken("Bearer_abc_user_guid");
        result.Should().BeNull();
    }

    /// <summary>
    /// Ветвь 5: корректный токен → возвращает userId
    /// </summary>
    [Fact]
    public void GetUserIdFromToken_ValidToken_ShouldReturnUserId()
    {
        var result = AuthController.GetUserIdFromToken("Bearer_42_user_someguid");
        result.Should().Be(42);
    }

    /// <summary>
    /// Ветвь 6: токен с префиксом "Bearer " (с пробелом)
    /// </summary>
    [Fact]
    public void GetUserIdFromToken_BearerWithSpace_ShouldParseCorrectly()
    {
        var result = AuthController.GetUserIdFromToken("Bearer Bearer_1_admin_guid");
        // После замены "Bearer " -> "" получаем "Bearer_1_admin_guid"
        // parts[1] = "1" → userId=1
        result.Should().Be(1);
    }

    // =============================================
    // GetUserRoleFromToken — анализ всех ветвей
    // =============================================

    /// <summary>
    /// Ветвь 1: null → null
    /// </summary>
    [Fact]
    public void GetUserRoleFromToken_NullInput_ShouldReturnNull()
    {
        var result = AuthController.GetUserRoleFromToken(null);
        result.Should().BeNull();
    }

    /// <summary>
    /// Ветвь 2: пустая строка → null
    /// </summary>
    [Fact]
    public void GetUserRoleFromToken_EmptyString_ShouldReturnNull()
    {
        var result = AuthController.GetUserRoleFromToken("");
        result.Should().BeNull();
    }

    /// <summary>
    /// Ветвь 3: менее 3 частей → null
    /// </summary>
    [Fact]
    public void GetUserRoleFromToken_TooFewParts_ShouldReturnNull()
    {
        var result = AuthController.GetUserRoleFromToken("Bearer_1");
        result.Should().BeNull();
    }

    /// <summary>
    /// Ветвь 4: роль "user"
    /// </summary>
    [Fact]
    public void GetUserRoleFromToken_UserRole_ShouldReturnUser()
    {
        var result = AuthController.GetUserRoleFromToken("Bearer_1_user_guid");
        result.Should().Be("user");
    }

    /// <summary>
    /// Ветвь 5: роль "admin"
    /// </summary>
    [Fact]
    public void GetUserRoleFromToken_AdminRole_ShouldReturnAdmin()
    {
        var result = AuthController.GetUserRoleFromToken("Bearer_1_admin_guid");
        result.Should().Be("admin");
    }

    // =============================================
    // Дополнительные тесты покрытия ветвей
    // =============================================

    /// <summary>
    /// Граничный случай: userId = 0
    /// </summary>
    [Fact]
    public void GetUserIdFromToken_ZeroUserId_ShouldReturnZero()
    {
        var result = AuthController.GetUserIdFromToken("Bearer_0_user_guid");
        result.Should().Be(0);
    }

    /// <summary>
    /// Граничный случай: отрицательный userId
    /// </summary>
    [Fact]
    public void GetUserIdFromToken_NegativeUserId_ShouldReturnNegative()
    {
        var result = AuthController.GetUserIdFromToken("Bearer_-1_user_guid");
        result.Should().Be(-1);
    }

    /// <summary>
    /// Граничный случай: очень большой userId
    /// </summary>
    [Fact]
    public void GetUserIdFromToken_LargeUserId_ShouldReturnLargeValue()
    {
        var result = AuthController.GetUserIdFromToken("Bearer_9999999999_user_guid");
        result.Should().Be(9999999999L);
    }
}

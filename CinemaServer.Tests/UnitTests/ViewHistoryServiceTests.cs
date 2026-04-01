using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.UnitTests;

/// <summary>
/// Модульные тесты для ViewHistoryService.
/// Тестируется запись и получение истории просмотров.
/// </summary>
public class ViewHistoryServiceTests
{
    /// <summary>
    /// Запись нового просмотра
    /// </summary>
    [Fact]
    public async Task RecordAsync_NewView_ShouldCreateRecord()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new ViewHistoryService(context);

        var result = await service.RecordAsync(2, 1, 600, false);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Обновление существующего просмотра — обновляет прогресс
    /// </summary>
    [Fact]
    public async Task RecordAsync_ExistingView_ShouldUpdateProgress()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new ViewHistoryService(context);

        var result = await service.RecordAsync(1, 1, 7200, true); // Обновляем прогресс user1, movie1

        result.Should().BeTrue();
        var history = context.ViewHistories.First(v => v.UserId == 1 && v.MovieId == 1);
        history.ProgressSeconds.Should().Be(7200);
        history.Completed.Should().BeTrue();
    }

    /// <summary>
    /// Получение истории просмотров пользователя
    /// </summary>
    [Fact]
    public async Task GetUserHistoryAsync_ShouldReturnHistory()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new ViewHistoryService(context);

        var history = await service.GetUserHistoryAsync(1);

        history.Should().HaveCount(1);
        history[0].Title.Should().Be("Тестовый фильм 1");
    }

    /// <summary>
    /// Получение истории — пустой список если нет записей
    /// </summary>
    [Fact]
    public async Task GetUserHistoryAsync_NoHistory_ShouldReturnEmpty()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new ViewHistoryService(context);

        var history = await service.GetUserHistoryAsync(3); // admin без истории

        history.Should().BeEmpty();
    }
}

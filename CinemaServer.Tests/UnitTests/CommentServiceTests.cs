using CinemaServer.Services;
using CinemaServer.Tests.Helpers;
using FluentAssertions;

namespace CinemaServer.Tests.UnitTests;

/// <summary>
/// Модульные тесты для CommentService.
/// Тестируется создание, получение и удаление комментариев.
/// </summary>
public class CommentServiceTests
{
    /// <summary>
    /// Создание комментария — возвращает ID нового комментария
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldCreateComment_AndReturnId()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new CommentService(context);

        var commentId = await service.CreateAsync(1, 2, "Новый комментарий");

        commentId.Should().BeGreaterThan(0);
        var comment = await context.Comments.FindAsync(commentId);
        comment.Should().NotBeNull();
        comment!.Content.Should().Be("Новый комментарий");
        comment.IsVisible.Should().BeTrue();
    }

    /// <summary>
    /// Получение комментариев к фильму — только видимые
    /// </summary>
    [Fact]
    public async Task GetByMovieIdAsync_ShouldReturnOnlyVisibleComments()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new CommentService(context);

        var comments = await service.GetByMovieIdAsync(1);

        comments.Should().HaveCount(2); // 2 видимых комментария к фильму 1
        comments.Should().OnlyContain(c => c.Content != "Скрытый комментарий");
    }

    /// <summary>
    /// Получение комментариев к фильму без комментариев — пустой список
    /// </summary>
    [Fact]
    public async Task GetByMovieIdAsync_NoComments_ShouldReturnEmpty()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new CommentService(context);

        var comments = await service.GetByMovieIdAsync(999);

        comments.Should().BeEmpty();
    }

    /// <summary>
    /// Удаление собственного комментария — успех
    /// </summary>
    [Fact]
    public async Task DeleteAsync_OwnComment_ShouldDeleteAndReturnTrue()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new CommentService(context);

        var result = await service.DeleteAsync(1, 1, false); // userId=1, isAdmin=false

        result.Should().BeTrue();
        var comment = await context.Comments.FindAsync(1L);
        comment.Should().BeNull();
    }

    /// <summary>
    /// Удаление чужого комментария обычным пользователем — отказ
    /// </summary>
    [Fact]
    public async Task DeleteAsync_OtherUserComment_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new CommentService(context);

        var result = await service.DeleteAsync(2, 1, false); // commentId=2 принадлежит userId=2

        result.Should().BeFalse();
    }

    /// <summary>
    /// Удаление чужого комментария администратором — успех
    /// </summary>
    [Fact]
    public async Task DeleteAsync_AdminDeletingOtherComment_ShouldReturnTrue()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new CommentService(context);

        var result = await service.DeleteAsync(2, 1, true); // isAdmin=true

        result.Should().BeTrue();
    }

    /// <summary>
    /// Удаление несуществующего комментария — false
    /// </summary>
    [Fact]
    public async Task DeleteAsync_NonExistingComment_ShouldReturnFalse()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new CommentService(context);

        var result = await service.DeleteAsync(999, 1, true);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Комментарии возвращаются в порядке убывания даты
    /// </summary>
    [Fact]
    public async Task GetByMovieIdAsync_ShouldReturnOrderedByDateDesc()
    {
        var context = TestDbContextFactory.CreateWithSeedData();
        var service = new CommentService(context);

        var comments = await service.GetByMovieIdAsync(1);

        comments.Should().HaveCount(2);
        comments[0].CreatedAt.Should().BeOnOrAfter(comments[1].CreatedAt);
    }
}

using CinemaServer.DTOs;
using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Services;

public class CommentService
{
    private readonly CinemaOnlineContext _context;

    public CommentService(CinemaOnlineContext context)
    {
        _context = context;
    }

    public async Task<long> CreateAsync(long userId, long movieId, string content)
    {
        var comment = new Comment
        {
            UserId = userId,
            MovieId = movieId,
            Content = content,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment.Id;
    }

    public async Task<List<CommentResponse>> GetByMovieIdAsync(long movieId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.MovieId == movieId && c.IsVisible == true)
            .OrderByDescending(c => c.CreatedAt)
            .Take(100)
            .Select(c => new CommentResponse
            {
                Id = c.Id,
                UserId = c.UserId,
                UserName = c.User != null ? c.User.Name ?? "Unknown" : "Unknown",
                Content = c.Content,
                CreatedAt = c.CreatedAt ?? DateTime.UtcNow
            })
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(long commentId, long userId, bool isAdmin)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null) return false;

        if (!isAdmin && comment.UserId != userId) return false;

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }
}

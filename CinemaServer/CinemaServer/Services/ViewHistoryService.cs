using CinemaServer.DTOs;
using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Services;

public class ViewHistoryService
{
    private readonly CinemaOnlineContext _context;

    public ViewHistoryService(CinemaOnlineContext context)
    {
        _context = context;
    }

    public async Task<bool> RecordAsync(long userId, long movieId, int progressSeconds, bool completed = false)
    {
        var existing = await _context.ViewHistories
            .FirstOrDefaultAsync(v => v.UserId == userId && v.MovieId == movieId);

        if (existing != null)
        {
            existing.ProgressSeconds = progressSeconds;
            existing.Completed = completed;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.ViewHistories.Add(new ViewHistory
            {
                UserId = userId,
                MovieId = movieId,
                ProgressSeconds = progressSeconds,
                Completed = completed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<MovieResponse>> GetUserHistoryAsync(long userId)
    {
        return await _context.ViewHistories
            .Include(v => v.Movie)
                .ThenInclude(m => m.Genres)
            .Where(v => v.UserId == userId && v.Movie.IsPublished == true)
            .OrderByDescending(v => v.UpdatedAt)
            .Take(50)
            .Select(v => new MovieResponse
            {
                Id = v.Movie.Id,
                Title = v.Movie.Title,
                Description = v.Movie.Description,
                ReleaseYear = v.Movie.ReleaseYear,
                DurationMinutes = v.Movie.DurationMinutes,
                PosterUrl = v.Movie.PosterUrl,
                Country = v.Movie.Country,
                Director = v.Movie.Director,
                NeedSubscription = v.Movie.NeedSubscription ?? false,
                AverageRating = v.Movie.AverageRating ?? 0,
                RatingsCount = v.Movie.RatingsCount ?? 0,
                ViewCount = v.Movie.ViewCount ?? 0,
                CommentCount = v.Movie.CommentCount ?? 0,
                CreatedAt = v.Movie.CreatedAt ?? DateTime.UtcNow,
                Genres = v.Movie.Genres.Select(g => g.DisplayName).ToList()
            })
            .ToListAsync();
    }
}

using CinemaServer.DTOs;
using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Services;

public class FavoriteService
{
    private readonly CinemaOnlineContext _context;

    public FavoriteService(CinemaOnlineContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(long userId, long movieId)
    {
        var exists = await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.MovieId == movieId);

        if (exists) return true;

        _context.Favorites.Add(new Favorite
        {
            UserId = userId,
            MovieId = movieId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(long userId, long movieId)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.MovieId == movieId);

        if (favorite == null) return false;

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<MovieResponse>> GetUserFavoritesAsync(long userId)
    {
        return await _context.Favorites
            .Include(f => f.Movie)
                .ThenInclude(m => m.Genres)
            .Where(f => f.UserId == userId && f.Movie.IsPublished == true)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new MovieResponse
            {
                Id = f.Movie.Id,
                Title = f.Movie.Title,
                Description = f.Movie.Description,
                ReleaseYear = f.Movie.ReleaseYear,
                DurationMinutes = f.Movie.DurationMinutes,
                PosterUrl = f.Movie.PosterUrl,
                Country = f.Movie.Country,
                Director = f.Movie.Director,
                NeedSubscription = f.Movie.NeedSubscription ?? false,
                AverageRating = f.Movie.AverageRating ?? 0,
                RatingsCount = f.Movie.RatingsCount ?? 0,
                ViewCount = f.Movie.ViewCount ?? 0,
                CommentCount = f.Movie.CommentCount ?? 0,
                CreatedAt = f.Movie.CreatedAt ?? DateTime.UtcNow,
                Genres = f.Movie.Genres.Select(g => g.DisplayName).ToList()
            })
            .ToListAsync();
    }
}

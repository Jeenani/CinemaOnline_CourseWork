using CinemaServer.DTOs;
using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Services;

public class RatingService
{
    private readonly CinemaOnlineContext _context;

    public RatingService(CinemaOnlineContext context)
    {
        _context = context;
    }

    public async Task<bool> RateMovieAsync(long userId, long movieId, int rating)
    {
        var existing = await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId);

        if (existing != null)
        {
            existing.Rating1 = rating;
        }
        else
        {
            _context.Ratings.Add(new Rating
            {
                UserId = userId,
                MovieId = movieId,
                Rating1 = rating
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int?> GetUserRatingAsync(long userId, long movieId)
    {
        return await _context.Ratings
            .Where(r => r.UserId == userId && r.MovieId == movieId)
            .Select(r => (int?)r.Rating1)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteRatingAsync(long userId, long movieId)
    {
        var rating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId);

        if (rating == null) return false;

        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync();
        return true;
    }
}

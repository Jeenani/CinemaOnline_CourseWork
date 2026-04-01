using CinemaServer.DTOs;
using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Services;

public class CollectionService
{
    private readonly CinemaOnlineContext _context;

    public CollectionService(CinemaOnlineContext context)
    {
        _context = context;
    }

    public async Task<List<CollectionResponse>> GetFeaturedAsync()
    {
        var collections = await _context.Collections
            .AsSplitQuery() // Разделяем запрос чтобы избежать предупреждения о множественных коллекциях
            .Include(c => c.CollectionMovies)
                .ThenInclude(cm => cm.Movie)
                    .ThenInclude(m => m.Genres)
            .Where(c => c.IsFeatured == true)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.CreatedAt)
            .ToListAsync();

        return collections.Select(c => new CollectionResponse
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Movies = c.CollectionMovies
                .Where(cm => cm.Movie.IsPublished == true)
                .OrderBy(cm => cm.Position)
                .Take(10)
                .Select(cm => new MovieResponse
                {
                    Id = cm.Movie.Id,
                    Title = cm.Movie.Title,
                    Description = cm.Movie.Description,
                    ReleaseYear = cm.Movie.ReleaseYear,
                    DurationMinutes = cm.Movie.DurationMinutes,
                    PosterUrl = cm.Movie.PosterUrl,
                    Country = cm.Movie.Country,
                    Director = cm.Movie.Director,
                    NeedSubscription = cm.Movie.NeedSubscription ?? false,
                    AverageRating = cm.Movie.AverageRating ?? 0,
                    RatingsCount = cm.Movie.RatingsCount ?? 0,
                    ViewCount = cm.Movie.ViewCount ?? 0,
                    CommentCount = cm.Movie.CommentCount ?? 0,
                    CreatedAt = cm.Movie.CreatedAt ?? DateTime.UtcNow,
                    Genres = cm.Movie.Genres.Select(g => g.DisplayName).ToList()
                })
                .ToList()
        }).ToList();
    }
}

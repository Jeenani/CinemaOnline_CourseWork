using CinemaServer.DTOs;
using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Services;

public class MovieService
{
    private readonly CinemaOnlineContext _context;

    public MovieService(CinemaOnlineContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<MovieResponse>> SearchAsync(MovieSearchRequest request)
    {
        var query = _context.Movies
            .Include(m => m.Genres)
            .Where(m => m.IsPublished == true)
            .AsQueryable();

        // Поиск по названию
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(m => m.Title.ToLower().Contains(search) || 
                                     (m.Description != null && m.Description.ToLower().Contains(search)));
        }

        // Фильтр по жанру
        if (request.GenreId.HasValue)
        {
            query = query.Where(m => m.Genres.Any(g => g.Id == request.GenreId.Value));
        }

        // Фильтр по году
        if (request.YearFrom.HasValue)
        {
            query = query.Where(m => m.ReleaseYear >= request.YearFrom.Value);
        }
        if (request.YearTo.HasValue)
        {
            query = query.Where(m => m.ReleaseYear <= request.YearTo.Value);
        }

        // Фильтр по стране
        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            query = query.Where(m => m.Country != null && m.Country.ToLower().Contains(request.Country.ToLower()));
        }

        // Фильтр по минимальному рейтингу
        if (request.MinRating.HasValue)
        {
            query = query.Where(m => m.AverageRating >= request.MinRating.Value);
        }

        // Сортировка
        query = request.SortBy.ToLower() switch
        {
            "rating" => request.SortDescending 
                ? query.OrderByDescending(m => m.AverageRating) 
                : query.OrderBy(m => m.AverageRating),
            "views" => request.SortDescending 
                ? query.OrderByDescending(m => m.ViewCount) 
                : query.OrderBy(m => m.ViewCount),
            "title" => request.SortDescending 
                ? query.OrderByDescending(m => m.Title) 
                : query.OrderBy(m => m.Title),
            "year" => request.SortDescending 
                ? query.OrderByDescending(m => m.ReleaseYear) 
                : query.OrderBy(m => m.ReleaseYear),
            _ => request.SortDescending 
                ? query.OrderByDescending(m => m.CreatedAt) 
                : query.OrderBy(m => m.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var movies = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResponse<MovieResponse>
        {
            Items = movies.Select(MapToResponse).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<MovieResponse?> GetRandomAsync()
    {
        var movie = await _context.Movies
            .Include(m => m.Genres)
            .Where(m => m.IsPublished == true && m.BannerUrl != null)
            .OrderBy(m => Guid.NewGuid())
            .FirstOrDefaultAsync();

        if (movie == null)
        {
            movie = await _context.Movies
                .Include(m => m.Genres)
                .Where(m => m.IsPublished == true)
                .OrderBy(m => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }

        return movie == null ? null : MapToResponse(movie);
    }

    public async Task<List<string>> GetCountriesAsync()
    {
        return await _context.Movies
            .Where(m => m.IsPublished == true && m.Country != null)
            .Select(m => m.Country!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<MovieDetailResponse?> GetByIdAsync(long movieId, long? userId = null)
    {
        var movie = await _context.Movies
            .AsSplitQuery()
            .Include(m => m.Genres)
            .Include(m => m.Comments.Where(c => c.IsVisible == true))
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(m => m.Id == movieId && m.IsPublished == true);

        if (movie == null) return null;

        var detail = new MovieDetailResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseYear = movie.ReleaseYear,
            DurationMinutes = movie.DurationMinutes,
            VideoUrl = movie.VideoUrl,
            VkVideoUrl = movie.VkVideoUrl,
            PosterUrl = movie.PosterUrl,
            BannerUrl = movie.BannerUrl,
            Country = movie.Country,
            Director = movie.Director,
            NeedSubscription = movie.NeedSubscription ?? false,
            AverageRating = movie.AverageRating ?? 0,
            RatingsCount = movie.RatingsCount ?? 0,
            ViewCount = movie.ViewCount ?? 0,
            CommentCount = movie.CommentCount ?? 0,
            CreatedAt = movie.CreatedAt ?? DateTime.Now,
            Genres = movie.Genres.Select(g => g.DisplayName).ToList(),
            Comments = movie.Comments
                .OrderByDescending(c => c.CreatedAt)
                .Take(50)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.User?.Name ?? "Unknown",
                    Content = c.Content,
                    CreatedAt = c.CreatedAt ?? DateTime.Now
                }).ToList()
        };

        if (userId.HasValue)
        {
            detail.UserRating = await _context.Ratings
                .Where(r => r.MovieId == movieId && r.UserId == userId.Value)
                .Select(r => (int?)r.Rating1)
                .FirstOrDefaultAsync();

            detail.IsFavorite = await _context.Favorites
                .AnyAsync(f => f.MovieId == movieId && f.UserId == userId.Value);
        }

        return detail;
    }

    public async Task<long> CreateAsync(CreateMovieRequest request)
    {
        var genres = await _context.Genres
            .Where(g => request.GenreIds.Contains(g.Id))
            .ToListAsync();

        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            ReleaseYear = request.ReleaseYear,
            DurationMinutes = request.DurationMinutes,
            VideoUrl = request.VideoUrl,
            VkVideoUrl = request.VkVideoUrl,
            PosterUrl = request.PosterUrl,
            BannerUrl = request.BannerUrl,
            Country = request.Country,
            Director = request.Director,
            NeedSubscription = request.NeedSubscription,
            IsPublished = true,
            Genres = genres,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        return movie.Id;
    }

    public async Task<bool> UpdateAsync(long movieId, UpdateMovieRequest request)
    {
        var movie = await _context.Movies
            .Include(m => m.Genres)
            .FirstOrDefaultAsync(m => m.Id == movieId);

        if (movie == null) return false;

        if (request.Title != null) movie.Title = request.Title;
        if (request.Description != null) movie.Description = request.Description;
        if (request.ReleaseYear.HasValue) movie.ReleaseYear = request.ReleaseYear;
        if (request.DurationMinutes.HasValue) movie.DurationMinutes = request.DurationMinutes;
        if (request.VideoUrl != null) movie.VideoUrl = request.VideoUrl;
        if (request.VkVideoUrl != null) movie.VkVideoUrl = request.VkVideoUrl;
        if (request.PosterUrl != null) movie.PosterUrl = request.PosterUrl;
        if (request.BannerUrl != null) movie.BannerUrl = request.BannerUrl;
        if (request.Country != null) movie.Country = request.Country;
        if (request.Director != null) movie.Director = request.Director;
        if (request.NeedSubscription.HasValue) movie.NeedSubscription = request.NeedSubscription;
        if (request.IsPublished.HasValue) movie.IsPublished = request.IsPublished;

        if (request.GenreIds != null)
        {
            movie.Genres.Clear();
            var genres = await _context.Genres
                .Where(g => request.GenreIds.Contains(g.Id))
                .ToListAsync();
            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }
        }

        movie.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long movieId)
    {
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null) return false;

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
        return true;
    }

    private static MovieResponse MapToResponse(Movie movie) => new()
    {
        Id = movie.Id,
        Title = movie.Title,
        Description = movie.Description,
        ReleaseYear = movie.ReleaseYear,
        DurationMinutes = movie.DurationMinutes,
        VkVideoUrl = movie.VkVideoUrl,
        PosterUrl = movie.PosterUrl,
        BannerUrl = movie.BannerUrl,
        Country = movie.Country,
        Director = movie.Director,
        NeedSubscription = movie.NeedSubscription ?? false,
        AverageRating = movie.AverageRating ?? 0,
        RatingsCount = movie.RatingsCount ?? 0,
        ViewCount = movie.ViewCount ?? 0,
        CommentCount = movie.CommentCount ?? 0,
        CreatedAt = movie.CreatedAt ?? DateTime.Now,
        Genres = movie.Genres.Select(g => g.DisplayName).ToList()
    };
}

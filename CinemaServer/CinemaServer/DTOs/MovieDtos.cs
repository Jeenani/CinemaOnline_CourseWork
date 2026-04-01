using System.ComponentModel.DataAnnotations;

namespace CinemaServer.DTOs;

// ============================================
// MOVIE DTOs
// ============================================

public class MovieSearchRequest
{
    public string? Search { get; set; }
    public long? GenreId { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public string? Country { get; set; }
    public decimal? MinRating { get; set; }
    public string SortBy { get; set; } = "date"; // date, rating, views, title
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class MovieResponse
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int? DurationMinutes { get; set; }
    public string? PosterUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? Country { get; set; }
    public string? Director { get; set; }
    public string? VkVideoUrl { get; set; }
    public bool NeedSubscription { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingsCount { get; set; }
    public long ViewCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Genres { get; set; } = new();
}

public class MovieDetailResponse : MovieResponse
{
    public string VideoUrl { get; set; } = string.Empty;
    public int? UserRating { get; set; }
    public bool IsFavorite { get; set; }
    public List<CommentResponse> Comments { get; set; } = new();
}

public class CreateMovieRequest
{
    [Required, MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int? DurationMinutes { get; set; }
    
    [Required]
    public string VideoUrl { get; set; } = string.Empty;

    public string? VkVideoUrl { get; set; }
    
    public string? PosterUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? Country { get; set; }
    public string? Director { get; set; }
    public bool NeedSubscription { get; set; }
    public List<long> GenreIds { get; set; } = new();
}

public class UpdateMovieRequest
{
    [MaxLength(255)]
    public string? Title { get; set; }
    
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int? DurationMinutes { get; set; }
    public string? VideoUrl { get; set; }
    public string? VkVideoUrl { get; set; }
    public string? PosterUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? Country { get; set; }
    public string? Director { get; set; }
    public bool? NeedSubscription { get; set; }
    public bool? IsPublished { get; set; }
    public List<long>? GenreIds { get; set; }
}

// ============================================
// RATING & COMMENT DTOs
// ============================================

public class RateMovieRequest
{
    [Range(1, 5)]
    public int Rating { get; set; }
}

public class CreateCommentRequest
{
    [Required, MinLength(1), MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

public class CommentResponse
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ============================================
// VIEW HISTORY DTO
// ============================================

public class RecordViewRequest
{
    public int ProgressSeconds { get; set; }
    public bool Completed { get; set; }
}

namespace CinemaServer.DTOs;

// ============================================
// WEB-ONLY DTOs (used by Blazor admin pages)
// ============================================

public class AdminStats
{
    public int MoviesCount { get; set; }
    public int UsersCount { get; set; }
    public int CommentsCount { get; set; }
    public int GenresCount { get; set; }
    public int CollectionsCount { get; set; }
    public int ActiveSubscriptions { get; set; }
    public long TotalViews { get; set; }
    public int PaidPayments { get; set; }
}

public class AdminUser
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Role { get; set; }
    public bool HasSubscription { get; set; }
    public string? SubscriptionName { get; set; }
    public DateTime? SubscriptionEnd { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class AdminMovie
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string? Country { get; set; }
    public string? Director { get; set; }
    public string? PosterUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? VkVideoUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public bool IsPublished { get; set; }
    public bool NeedSubscription { get; set; }
    public decimal AverageRating { get; set; }
    public long ViewCount { get; set; }
    public int CommentCount { get; set; }
    public List<string> Genres { get; set; } = new();
    public List<long> GenreIds { get; set; } = new();
}

public class AdminComment
{
    public long Id { get; set; }
    public string? UserName { get; set; }
    public string? MovieTitle { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class AdminCollection
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public int MoviesCount { get; set; }
}

public class MovieFormModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int? DurationMinutes { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public string? VkVideoUrl { get; set; }
    public string? PosterUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? Country { get; set; }
    public string? Director { get; set; }
    public bool NeedSubscription { get; set; }
    public List<long> GenreIds { get; set; } = new();
}

// ============================================
// USER SETTINGS DTOs
// ============================================

public class UpdateProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

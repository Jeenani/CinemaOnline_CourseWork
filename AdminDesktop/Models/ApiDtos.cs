namespace AdminDesktop.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserInfo User { get; set; } = null!;
}

public class UserInfo
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

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

public class AdminMovie
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int? DurationMinutes { get; set; }
    public string? VideoUrl { get; set; }
    public string? VkVideoUrl { get; set; }
    public string? PosterUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? Country { get; set; }
    public string? Director { get; set; }
    public bool IsPublished { get; set; }
    public bool NeedSubscription { get; set; }
    public decimal AverageRating { get; set; }
    public long ViewCount { get; set; }
    public int CommentCount { get; set; }
    public List<string> Genres { get; set; } = new();
    public List<long> GenreIds { get; set; } = new();
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

public class AdminComment
{
    public long Id { get; set; }
    public string? UserName { get; set; }
    public string? MovieTitle { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class GenreItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
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

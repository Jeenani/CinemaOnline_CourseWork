using System;
using System.Collections.Generic;

namespace CinemaServer.Models;

public partial class Movie
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? ReleaseYear { get; set; }

    public int? DurationMinutes { get; set; }

    public string VideoUrl { get; set; } = null!;

    public string? VkVideoUrl { get; set; }

    public string? PosterUrl { get; set; }

    /// <summary>
    /// Большая картинка-баннер для превью на главной странице
    /// </summary>
    public string? BannerUrl { get; set; }

    public string? Country { get; set; }

    public string? Director { get; set; }

    public bool? NeedSubscription { get; set; }

    public bool? IsPublished { get; set; }

    public decimal? AverageRating { get; set; }

    public int? RatingsCount { get; set; }

    public long? ViewCount { get; set; }

    public int? CommentCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public virtual ICollection<CollectionMovie> CollectionMovies { get; set; } = new List<CollectionMovie>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<ViewHistory> ViewHistories { get; set; } = new List<ViewHistory>();

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}

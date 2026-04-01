using System;
using System.Collections.Generic;

namespace CinemaServer.Models;

public partial class VMoviesWithGenre
{
    public long? Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? ReleaseYear { get; set; }

    public int? DurationMinutes { get; set; }

    public string? PosterUrl { get; set; }

    public decimal? AverageRating { get; set; }

    public int? RatingsCount { get; set; }

    public long? ViewCount { get; set; }

    public bool? NeedSubscription { get; set; }

    public List<string>? Genres { get; set; }
}

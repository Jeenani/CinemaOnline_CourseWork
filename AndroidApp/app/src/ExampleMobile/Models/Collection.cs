using System;
using System.Collections.Generic;

namespace CinemaServer.Models;

public partial class Collection
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsFeatured { get; set; }

    public int? DisplayOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CollectionMovie> CollectionMovies { get; set; } = new List<CollectionMovie>();
}

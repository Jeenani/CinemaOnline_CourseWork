using System;
using System.Collections.Generic;

namespace CinemaServer.Models;

public partial class Genre
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
}

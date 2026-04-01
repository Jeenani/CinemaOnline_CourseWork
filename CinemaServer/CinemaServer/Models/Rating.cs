using System;
using System.Collections.Generic;

namespace CinemaServer.Models;

public partial class Rating
{
    public long UserId { get; set; }

    public long MovieId { get; set; }

    public int Rating1 { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Movie Movie { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

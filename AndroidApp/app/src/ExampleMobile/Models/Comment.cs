using System;
using System.Collections.Generic;

namespace CinemaServer.Models;

public partial class Comment
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long MovieId { get; set; }

    public string Content { get; set; } = null!;

    public bool? IsVisible { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Movie Movie { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

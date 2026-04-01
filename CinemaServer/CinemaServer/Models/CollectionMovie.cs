using System;
using System.Collections.Generic;

namespace CinemaServer.Models;

public partial class CollectionMovie
{
    public long CollectionId { get; set; }

    public long MovieId { get; set; }

    public int? Position { get; set; }

    public virtual Collection Collection { get; set; } = null!;

    public virtual Movie Movie { get; set; } = null!;
}

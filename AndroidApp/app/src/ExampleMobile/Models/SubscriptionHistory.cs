using System;
using System.Collections.Generic;

namespace CinemaServer.Models;

public partial class SubscriptionHistory
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long SubscriptionId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Subscription Subscription { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

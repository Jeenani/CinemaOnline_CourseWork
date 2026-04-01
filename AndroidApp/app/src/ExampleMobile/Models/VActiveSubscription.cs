using System;
using System.Collections.Generic;

namespace CinemaServer.Models;

public partial class VActiveSubscription
{
    public long? UserId { get; set; }

    public string? Email { get; set; }

    public string? Name { get; set; }

    public string? SubscriptionName { get; set; }

    public DateTime? SubscriptionStartDate { get; set; }

    public DateTime? SubscriptionEndDate { get; set; }

    public bool? HasSubscription { get; set; }
}

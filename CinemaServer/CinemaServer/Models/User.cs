using System;
using System.Collections.Generic;

namespace CinemaServer.Models;

public partial class User
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Name { get; set; }

    public string? Role { get; set; }

    public long? SubscriptionId { get; set; }

    public bool? HasSubscription { get; set; }

    public DateTime? SubscriptionStartDate { get; set; }

    public DateTime? SubscriptionEndDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual Subscription? Subscription { get; set; }

    public virtual ICollection<SubscriptionHistory> SubscriptionHistories { get; set; } = new List<SubscriptionHistory>();

    public virtual ICollection<ViewHistory> ViewHistories { get; set; } = new List<ViewHistory>();
}

using System;
using System.Collections.Generic;

namespace CinemaServer.Models;

public partial class Payment
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long SubscriptionId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Subscription Subscription { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

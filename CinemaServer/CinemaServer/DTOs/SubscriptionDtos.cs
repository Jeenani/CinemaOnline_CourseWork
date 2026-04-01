using System.ComponentModel.DataAnnotations;

namespace CinemaServer.DTOs;

// ============================================
// SUBSCRIPTION DTOs
// ============================================

public class SubscriptionPlanResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

// ============================================
// PAYMENT DTOs
// ============================================

public class CreatePaymentRequest
{
    [Required]
    public long SubscriptionId { get; set; }
    
    [Required]
    public string PaymentMethod { get; set; } = string.Empty; // card, paypal, etc.
}

public class ProcessPaymentRequest
{
    public bool Success { get; set; }
}

public class PaymentResponse
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long SubscriptionId { get; set; }
    public string SubscriptionName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============================================
// GENRE DTO
// ============================================

public class GenreResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

// ============================================
// COLLECTION DTOs
// ============================================

public class CollectionResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<MovieResponse> Movies { get; set; } = new();
}

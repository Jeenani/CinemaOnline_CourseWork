using System.ComponentModel.DataAnnotations;

namespace CinemaServer.DTOs;

// ============================================
// AUTH DTOs
// ============================================

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserResponse User { get; set; } = null!;
}

// ============================================
// USER DTOs
// ============================================

public class UserResponse
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool HasSubscription { get; set; }
    public SubscriptionInfo? Subscription { get; set; }
}

public class SubscriptionInfo
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

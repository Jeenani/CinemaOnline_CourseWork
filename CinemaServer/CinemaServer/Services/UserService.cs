using CinemaServer.DTOs;
using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Services;

public class UserService
{
    private readonly CinemaOnlineContext _context;

    public UserService(CinemaOnlineContext context)
    {
        _context = context;
    }

    public async Task<UserResponse?> GetByIdAsync(long userId)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        // Автоматически деактивируем просроченную подписку
        if (user.HasSubscription == true && user.SubscriptionEndDate <= DateTime.Now)
        {
            await DeactivateSubscriptionAsync(user);
        }

        return MapToResponse(user);
    }

    public async Task<UserResponse?> GetByEmailAsync(string email)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null) return null;

        // Автоматически деактивируем просроченную подписку
        if (user.HasSubscription == true && user.SubscriptionEndDate <= DateTime.Now)
        {
            await DeactivateSubscriptionAsync(user);
        }

        return MapToResponse(user);
    }

    public async Task<string?> GetPasswordHashAsync(string email)
    {
        return await _context.Users
            .Where(u => u.Email == email)
            .Select(u => u.PasswordHash)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetPasswordHashByIdAsync(long userId)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.PasswordHash)
            .FirstOrDefaultAsync();
    }

    public async Task<long> CreateAsync(string email, string passwordHash, string name)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            Name = name,
            Role = "user",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user.Id;
    }

    public async Task<bool> UpdateProfileAsync(long userId, string name, string email)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        // Проверяем, не занят ли email другим пользователем
        if (user.Email != email)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == email && u.Id != userId);
            if (exists) return false;
        }

        user.Name = name;
        user.Email = email;
        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(long userId, string newPasswordHash)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.PasswordHash = newPasswordHash;
        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateSubscriptionAsync(long userId, long subscriptionId, int durationDays)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.SubscriptionId = subscriptionId;
        user.SubscriptionStartDate = DateTime.Now;
        user.SubscriptionEndDate = DateTime.Now.AddDays(durationDays);
        user.HasSubscription = true;
        user.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Проверяет наличие АКТИВНОЙ подписки (не просроченной)
    /// </summary>
    public async Task<bool> HasActiveSubscriptionAsync(long userId)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        if (user.HasSubscription != true || user.SubscriptionEndDate == null)
            return false;

        // Если подписка просрочена — деактивируем
        if (user.SubscriptionEndDate <= DateTime.Now)
        {
            await DeactivateSubscriptionAsync(user);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Проверяет, что у пользователя PREMIUM подписка (не basic).
    /// Фильмы с need_subscription=true требуют именно premium.
    /// </summary>
    public async Task<bool> HasPremiumSubscriptionAsync(long userId)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        if (user.HasSubscription != true || user.SubscriptionEndDate == null)
            return false;

        // Если подписка просрочена — деактивируем
        if (user.SubscriptionEndDate <= DateTime.Now)
        {
            await DeactivateSubscriptionAsync(user);
            return false;
        }

        // Проверяем, что подписка именно premium (по имени, регистронезависимо)
        if (user.Subscription == null) return false;

        var subName = user.Subscription.Name.ToLower();
        return subName.Contains("premium") || subName.Contains("про") || subName.Contains("pro");
    }

    /// <summary>
    /// Деактивирует просроченные подписки у всех пользователей.
    /// Вызывается фоновым сервисом.
    /// </summary>
    public async Task<int> DeactivateExpiredSubscriptionsAsync()
    {
        var now = DateTime.Now;
        var expiredUsers = await _context.Users
            .Where(u => u.HasSubscription == true && u.SubscriptionEndDate != null && u.SubscriptionEndDate <= now)
            .ToListAsync();

        foreach (var user in expiredUsers)
        {
            user.HasSubscription = false;
            user.SubscriptionId = null;
            user.SubscriptionStartDate = null;
            user.SubscriptionEndDate = null;
            user.UpdatedAt = DateTime.Now;
        }

        if (expiredUsers.Count > 0)
        {
            // Также обновляем историю подписок
            var expiredHistories = await _context.SubscriptionHistories
                .Where(sh => sh.IsActive == true && sh.EndDate <= now)
                .ToListAsync();

            foreach (var h in expiredHistories)
            {
                h.IsActive = false;
            }

            await _context.SaveChangesAsync();
        }

        return expiredUsers.Count;
    }

    private async Task DeactivateSubscriptionAsync(User user)
    {
        user.HasSubscription = false;
        user.SubscriptionId = null;
        user.SubscriptionStartDate = null;
        user.SubscriptionEndDate = null;
        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    private static UserResponse MapToResponse(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Name = user.Name ?? string.Empty,
        Role = user.Role ?? "user",
        HasSubscription = user.HasSubscription ?? false,
        Subscription = (user.HasSubscription == true && user.Subscription != null) ? new SubscriptionInfo
        {
            Id = user.Subscription.Id,
            Name = user.Subscription.Name,
            StartDate = user.SubscriptionStartDate,
            EndDate = user.SubscriptionEndDate
        } : null
    };
}

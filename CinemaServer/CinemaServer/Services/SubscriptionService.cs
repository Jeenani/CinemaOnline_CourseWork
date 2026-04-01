using CinemaServer.DTOs;
using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Services;

public class SubscriptionService
{
    private readonly CinemaOnlineContext _context;

    public SubscriptionService(CinemaOnlineContext context)
    {
        _context = context;
    }

    public async Task<List<SubscriptionPlanResponse>> GetAllPlansAsync()
    {
        return await _context.Subscriptions
            .Where(s => s.IsActive == true)
            .OrderBy(s => s.Price)
            .Select(s => new SubscriptionPlanResponse
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                DurationDays = s.DurationDays,
                Description = s.Description,
                IsActive = s.IsActive ?? true
            })
            .ToListAsync();
    }

    public async Task<SubscriptionPlanResponse?> GetByIdAsync(long planId)
    {
        return await _context.Subscriptions
            .Where(s => s.Id == planId)
            .Select(s => new SubscriptionPlanResponse
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                DurationDays = s.DurationDays,
                Description = s.Description,
                IsActive = s.IsActive ?? true
            })
            .FirstOrDefaultAsync();
    }
}

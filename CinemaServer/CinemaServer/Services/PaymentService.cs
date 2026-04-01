using CinemaServer.DTOs;
using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Services;

public class PaymentService
{
    private readonly CinemaOnlineContext _context;
    private readonly UserService _userService;
    private readonly SubscriptionService _subscriptionService;

    public PaymentService(
        CinemaOnlineContext context,
        UserService userService,
        SubscriptionService subscriptionService)
    {
        _context = context;
        _userService = userService;
        _subscriptionService = subscriptionService;
    }

    public async Task<PaymentResponse> CreateAsync(long userId, long subscriptionId, string paymentMethod)
    {
        var plan = await _subscriptionService.GetByIdAsync(subscriptionId);
        if (plan == null)
            throw new ArgumentException("Subscription plan not found");

        var transactionId = $"txn_{Guid.NewGuid():N}";

        var payment = new Payment
        {
            UserId = userId,
            SubscriptionId = subscriptionId,
            Amount = plan.Price,
            Status = "pending",
            PaymentMethod = paymentMethod,
            TransactionId = transactionId
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return new PaymentResponse
        {
            Id = payment.Id,
            UserId = userId,
            SubscriptionId = subscriptionId,
            SubscriptionName = plan.Name,
            Amount = plan.Price,
            Status = "pending",
            TransactionId = transactionId,
            CreatedAt = payment.CreatedAt ?? DateTime.Now
        };
    }

    public async Task<bool> ProcessAsync(long paymentId, bool success)
    {
        var payment = await _context.Payments
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null || payment.Status != "pending")
            return false;

        payment.Status = success ? "paid" : "failed";
        payment.UpdatedAt = DateTime.Now;

        if (success && payment.Subscription != null)
        {
            await _userService.UpdateSubscriptionAsync(
                payment.UserId,
                payment.SubscriptionId,
                payment.Subscription.DurationDays);

            // Добавляем в историю подписок
            _context.SubscriptionHistories.Add(new SubscriptionHistory
            {
                UserId = payment.UserId,
                SubscriptionId = payment.SubscriptionId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(payment.Subscription.DurationDays),
                IsActive = true
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<PaymentResponse>> GetUserPaymentsAsync(long userId)
    {
        return await _context.Payments
            .Include(p => p.Subscription)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(50)
            .Select(p => new PaymentResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                SubscriptionId = p.SubscriptionId,
                SubscriptionName = p.Subscription != null ? p.Subscription.Name : "Unknown",
                Amount = p.Amount,
                Status = p.Status,
                TransactionId = p.TransactionId,
                CreatedAt = p.CreatedAt ?? DateTime.Now
            })
            .ToListAsync();
    }
}

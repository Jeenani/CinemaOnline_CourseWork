using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Services;

/// <summary>
/// Фоновый сервис, который периодически проверяет и деактивирует просроченные подписки.
/// Запускается автоматически при старте сервера.
/// </summary>
public class SubscriptionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public SubscriptionCleanupService(IServiceProvider serviceProvider, ILogger<SubscriptionCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SubscriptionCleanupService запущен. Интервал проверки: {Interval}", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                var deactivated = await userService.DeactivateExpiredSubscriptionsAsync();

                if (deactivated > 0)
                {
                    _logger.LogInformation("Деактивировано просроченных подписок: {Count}", deactivated);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке просроченных подписок");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}

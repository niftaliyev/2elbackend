using Microsoft.EntityFrameworkCore;

namespace TwoHandApp.BackgroundServices;

public class AdBoostService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public AdBoostService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var boosts = await db.UserAdPackages
                .Include(b => b.Ad)
                .Include(b => b.PackagePrice)
                .Where(b => b.BoostsRemaining > 0 && b.EndDate > now)
                .Where(b => b.LastBoostedAt <= now.AddHours(-b.PackagePrice.IntervalHours ?? 1))
                .ToListAsync(stoppingToken);

            foreach (var boost in boosts)
            {
                // поднимаем объявление
                boost.Ad.CreatedAt = now;

                // уменьшаем счетчик поднятий
                boost.BoostsRemaining--;

                // фиксируем время последнего поднятия
                boost.LastBoostedAt = now;
            }

            if (boosts.Count > 0)
                await db.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}

using ARBISTO_POS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class NotificationCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                var threshold = DateTime.Now.AddHours(-24);

                var oldNotifications = await context.Notifications
                    .Where(n => n.CreatedAt <= threshold)
                    .ToListAsync();

                if (oldNotifications.Any())
                {
                    context.Notifications.RemoveRange(oldNotifications);
                    await context.SaveChangesAsync();
                }
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
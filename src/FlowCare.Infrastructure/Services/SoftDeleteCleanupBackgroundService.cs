using FlowCare.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlowCare.Infrastructure.Services;

public class SoftDeleteCleanupBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<SoftDeleteCleanupBackgroundService> logger) : BackgroundService
{
    private const string SystemActorId = "system_cleanup_worker";
    private const string SystemActorRole = "System";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = configuration.GetValue<bool?>("CleanupWorker:Enabled") ?? true;
        if (!enabled)
        {
            logger.LogInformation("Soft-delete cleanup background worker is disabled.");
            return;
        }

        var intervalMinutes = configuration.GetValue<int?>("CleanupWorker:IntervalMinutes") ?? 60;
        if (intervalMinutes < 1)
            intervalMinutes = 1;

        logger.LogInformation(
            "Soft-delete cleanup background worker started with interval {IntervalMinutes} minute(s).",
            intervalMinutes);

        // Run once shortly after startup, then periodically.
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        await RunCleanupAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));
        while (!stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunCleanupAsync(stoppingToken);
        }
    }

    private async Task RunCleanupAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var maintenanceService = scope.ServiceProvider.GetRequiredService<IAdminMaintenanceService>();

            var result = await maintenanceService.CleanupSoftDeletedSlotsAsync(SystemActorId, SystemActorRole);

            if (result.DeletedSlots > 0 || result.UpdatedAppointments > 0)
            {
                logger.LogInformation(
                    "Soft-delete cleanup completed. DeletedSlots={DeletedSlots}, UpdatedAppointments={UpdatedAppointments}",
                    result.DeletedSlots,
                    result.UpdatedAppointments);
            }
            else
                logger.LogDebug("Soft-delete cleanup completed with no expired slots.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Graceful shutdown.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Soft-delete cleanup background run failed.");
        }
    }
}

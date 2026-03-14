using FlowCare.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FlowCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.BackgroundJobs;

public class SoftDeleteCleanupBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<SoftDeleteCleanupBackgroundService> logger) : BackgroundService
{
    private const string CleanupWorkerEnabledSettingKey = "CleanupWorkerEnabled";
    private const string SystemActorId = "system_cleanup_worker";
    private const string SystemActorRole = "System";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var defaultEnabled = configuration.GetValue<bool?>("CleanupWorker:Enabled") ?? true;

        var intervalMinutes = configuration.GetValue<int?>("CleanupWorker:IntervalMinutes") ?? 60;
        if (intervalMinutes < 1)
            intervalMinutes = 1;

        logger.LogInformation(
            "Soft-delete cleanup background worker started with interval {IntervalMinutes} minute(s).",
            intervalMinutes);

        // Run once shortly after startup, then periodically.
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        await RunCleanupIfEnabledAsync(defaultEnabled, stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));
        while (!stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunCleanupIfEnabledAsync(defaultEnabled, stoppingToken);
        }
    }

    private async Task RunCleanupIfEnabledAsync(bool defaultEnabled, CancellationToken stoppingToken)
    {
        var enabled = await IsCleanupWorkerEnabledAsync(defaultEnabled, stoppingToken);
        if (!enabled)
        {
            logger.LogDebug("Soft-delete cleanup skipped because worker is disabled in settings.");
            return;
        }

        await RunCleanupAsync(stoppingToken);
    }

    private async Task<bool> IsCleanupWorkerEnabledAsync(bool defaultEnabled, CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FlowCareDbContext>();

            var setting = await db.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key == CleanupWorkerEnabledSettingKey, stoppingToken);

            if (setting is null)
                return defaultEnabled;

            return bool.TryParse(setting.Value, out var enabled)
                ? enabled
                : defaultEnabled;
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return defaultEnabled;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read cleanup worker enabled setting. Falling back to default configuration.");
            return defaultEnabled;
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

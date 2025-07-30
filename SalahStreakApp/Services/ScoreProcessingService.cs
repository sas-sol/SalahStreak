using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Data;
using SalahStreakApp.Models;

namespace SalahStreakApp.Services;

public class ScoreProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScoreProcessingService> _logger;

    public ScoreProcessingService(IServiceProvider serviceProvider, ILogger<ScoreProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Score processing service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessNewScoresAsync();
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // Check every 2 minutes
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in score processing service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task ProcessNewScoresAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var scoringService = scope.ServiceProvider.GetRequiredService<AttendanceScoringService>();

        // Get unprocessed biometric logs
        var unprocessedLogs = await dbContext.BiometricLogs
            .Where(log => !log.IsProcessed)
            .Select(log => log.CheckInTime.Date)
            .Distinct()
            .ToListAsync();

        if (unprocessedLogs.Any())
        {
            _logger.LogInformation("Processing scores for {Count} days with new biometric data", unprocessedLogs.Count);
            
            foreach (var date in unprocessedLogs)
            {
                await scoringService.ProcessAttendanceScoresAsync(date, date);
            }

            // Mark logs as processed
            await dbContext.BiometricLogs
                .Where(log => !log.IsProcessed)
                .ExecuteUpdateAsync(setters => setters.SetProperty(log => log.IsProcessed, true));
        }
    }
} 
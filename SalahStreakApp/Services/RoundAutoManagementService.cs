using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Data;
using SalahStreakApp.Models;

namespace SalahStreakApp.Services;

public class RoundAutoManagementService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RoundAutoManagementService> _logger;

    public RoundAutoManagementService(IServiceProvider serviceProvider, ILogger<RoundAutoManagementService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Round auto-management service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRoundManagementAsync();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Check every hour
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in round auto-management service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task ProcessRoundManagementAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var roundService = scope.ServiceProvider.GetRequiredService<RoundManagementService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Auto-complete expired rounds
        await roundService.AutoCompleteExpiredRoundsAsync();

        // Check if we need to create a new round
        var currentRound = await roundService.GetCurrentRoundAsync();
        if (currentRound == null)
        {
            // No active round, check if we should create one
            var lastRound = await dbContext.Rounds
                .OrderByDescending(r => r.EndDate)
                .FirstOrDefaultAsync();

            if (lastRound == null || lastRound.EndDate.AddDays(1) <= DateTime.Today)
            {
                // Create new round starting today or next day
                var startDate = lastRound?.EndDate.AddDays(1) ?? DateTime.Today;
                var roundName = $"Round {DateTime.Now:yyyy-MM}";
                
                await roundService.CreateRoundAsync(roundName, startDate);
                _logger.LogInformation("Created new round {RoundName} starting {StartDate}", roundName, startDate);
            }
        }
    }
} 
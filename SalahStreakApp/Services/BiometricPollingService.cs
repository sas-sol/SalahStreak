using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Data;
using SalahStreakApp.Models;

namespace SalahStreakApp.Services;

public class BiometricPollingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BiometricPollingService> _logger;
    private readonly IConfiguration _configuration;

    public BiometricPollingService(
        IServiceProvider serviceProvider,
        ILogger<BiometricPollingService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollingInterval = _configuration.GetValue<int>("BioTimeApi:PollingIntervalMinutes", 5);
        
        _logger.LogInformation("Biometric polling service started with {Interval} minute interval", pollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollBiometricDataAsync();
                await Task.Delay(TimeSpan.FromMinutes(pollingInterval), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in biometric polling service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait 1 minute before retry
            }
        }
    }

    private async Task PollBiometricDataAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var bioTimeService = scope.ServiceProvider.GetRequiredService<BioTimeApiService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Get last processed timestamp
        var lastProcessed = await dbContext.BiometricLogs
            .OrderByDescending(x => x.CheckInTime)
            .Select(x => x.CheckInTime)
            .FirstOrDefaultAsync();

        // Fetch new check-ins
        var newCheckIns = await bioTimeService.FetchCheckInsAsync(lastProcessed);

        if (newCheckIns.Any())
        {
            // Match with participants
            var matchedCheckIns = await MatchWithParticipantsAsync(newCheckIns, dbContext);
            
            // Save to database
            await dbContext.BiometricLogs.AddRangeAsync(matchedCheckIns);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Processed {Count} new biometric check-ins", matchedCheckIns.Count);
        }
    }

    private async Task<List<BiometricLog>> MatchWithParticipantsAsync(List<BiometricLog> checkIns, ApplicationDbContext dbContext)
    {
        var matchedCheckIns = new List<BiometricLog>();

        foreach (var checkIn in checkIns)
        {
            // Try to match by ParticipantId (string)
            var participant = await dbContext.Participants
                .FirstOrDefaultAsync(p => p.ParticipantId == checkIn.ParticipantId);

            if (participant == null)
            {
                // Try to match by integer ID if ParticipantId is numeric
                if (int.TryParse(checkIn.ParticipantId, out var participantIdInt))
                {
                    participant = await dbContext.Participants
                        .FirstOrDefaultAsync(p => p.Id == participantIdInt);
                    
                    if (participant != null)
                    {
                        checkIn.ParticipantId_int = participantIdInt;
                    }
                }
            }

            if (participant != null)
            {
                checkIn.Participant = participant;
                matchedCheckIns.Add(checkIn);
                _logger.LogDebug("Matched check-in for participant {ParticipantId}", participant.ParticipantId);
            }
            else
            {
                _logger.LogWarning("No participant found for biometric check-in {ParticipantId}", checkIn.ParticipantId);
                // Still save unmatched check-ins for auditing
                matchedCheckIns.Add(checkIn);
            }
        }

        return matchedCheckIns;
    }
} 
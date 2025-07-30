using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Data;
using SalahStreakApp.Models;

namespace SalahStreakApp.Services;

public class RoundManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<RoundManagementService> _logger;
    private readonly AttendanceScoringService _scoringService;

    public RoundManagementService(
        ApplicationDbContext dbContext, 
        ILogger<RoundManagementService> logger,
        AttendanceScoringService scoringService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _scoringService = scoringService;
    }

    public async Task<Round> CreateRoundAsync(string name, DateTime startDate, int durationDays = 40)
    {
        var endDate = startDate.AddDays(durationDays - 1); // Inclusive end date
        
        var round = new Round
        {
            Name = name,
            StartDate = startDate,
            EndDate = endDate,
            DurationDays = durationDays,
            IsActive = true,
            IsCompleted = false,
            CreatedAt = DateTime.Now
        };

        _dbContext.Rounds.Add(round);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created round {RoundName} from {StartDate} to {EndDate}", 
            name, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

        return round;
    }

    public async Task ProcessRoundWinnersAsync(int roundId)
    {
        var round = await _dbContext.Rounds
            .Include(r => r.Winners)
            .FirstOrDefaultAsync(r => r.Id == roundId);

        if (round == null)
        {
            _logger.LogWarning("Round {RoundId} not found", roundId);
            return;
        }

        if (round.IsCompleted)
        {
            _logger.LogInformation("Round {RoundId} already completed", roundId);
            return;
        }

        _logger.LogInformation("Processing winners for round {RoundName}", round.Name);

        // Get all participants with their scores for this round
        var participantScores = await GetParticipantScoresForRoundAsync(round);

        // Group by age group and find winners
        var winners = new List<Winner>();
        var ageGroups = await _dbContext.AgeGroups.ToListAsync();

        foreach (var ageGroup in ageGroups)
        {
            var ageGroupWinners = await ProcessAgeGroupWinnersAsync(round, ageGroup, participantScores);
            winners.AddRange(ageGroupWinners);
        }

        // Save winners
        if (winners.Any())
        {
            await _dbContext.Winners.AddRangeAsync(winners);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Added {WinnerCount} winners to round {RoundName}", 
                winners.Count, round.Name);
        }

        // Mark round as completed
        round.IsCompleted = true;
        round.UpdatedAt = DateTime.Now;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Completed round {RoundName} with {WinnerCount} winners", 
            round.Name, winners.Count);
    }

    private async Task<List<Winner>> ProcessAgeGroupWinnersAsync(
        Round round, 
        AgeGroup ageGroup, 
        Dictionary<int, int> participantScores)
    {
        var winners = new List<Winner>();

        // Get participants in this age group with scores >= 195
        var eligibleParticipants = await _dbContext.Participants
            .Where(p => p.AgeGroupId == ageGroup.Id)
            .Where(p => participantScores.ContainsKey(p.Id) && participantScores[p.Id] >= 195)
            .OrderByDescending(p => participantScores[p.Id])
            .ThenBy(p => p.ParticipantId) // Tie-breaker
            .ToListAsync();

        if (!eligibleParticipants.Any())
        {
            _logger.LogDebug("No eligible winners in age group {AgeGroupName}", ageGroup.Name);
            return winners;
        }

        // Create winner records with ranking
        for (int i = 0; i < eligibleParticipants.Count; i++)
        {
            var participant = eligibleParticipants[i];
            var score = participantScores[participant.Id];

            var winner = new Winner
            {
                RoundId = round.Id,
                ParticipantId = participant.Id,
                AgeGroupId = ageGroup.Id,
                FinalScore = score,
                RankInAgeGroup = i + 1,
                IsRewarded = false,
                CreatedAt = DateTime.Now
            };

            winners.Add(winner);

            _logger.LogDebug("Added winner: {ParticipantName} (Age Group: {AgeGroupName}, Score: {Score}, Rank: {Rank})", 
                participant.FullName, ageGroup.Name, score, i + 1);
        }

        return winners;
    }

    private async Task<Dictionary<int, int>> GetParticipantScoresForRoundAsync(Round round)
    {
        // Get all attendance scores for the round period
        var scores = await _dbContext.AttendanceScores
            .Include(score => score.AttendanceCalendar)
            .Where(score => score.AttendanceCalendar.Date >= round.StartDate && 
                           score.AttendanceCalendar.Date <= round.EndDate &&
                           score.AttendanceCalendar.IsActive)
            .ToListAsync();

        // Group by participant and sum scores
        var participantScores = scores
            .GroupBy(score => score.ParticipantId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(score => score.Score)
            );

        return participantScores;
    }

    public async Task<List<Winner>> GetRoundWinnersAsync(int roundId, int? ageGroupId = null)
    {
        var query = _dbContext.Winners
            .Include(w => w.Participant)
            .Include(w => w.AgeGroup)
            .Include(w => w.Round)
            .Where(w => w.RoundId == roundId);

        if (ageGroupId.HasValue)
        {
            query = query.Where(w => w.AgeGroupId == ageGroupId.Value);
        }

        return await query
            .OrderBy(w => w.AgeGroup.Name)
            .ThenBy(w => w.RankInAgeGroup)
            .ToListAsync();
    }

    public async Task<List<Round>> GetActiveRoundsAsync()
    {
        return await _dbContext.Rounds
            .Where(r => r.IsActive && !r.IsCompleted)
            .OrderByDescending(r => r.StartDate)
            .ToListAsync();
    }

    public async Task<List<Round>> GetCompletedRoundsAsync()
    {
        return await _dbContext.Rounds
            .Where(r => r.IsCompleted)
            .OrderByDescending(r => r.EndDate)
            .ToListAsync();
    }

    public async Task<Round?> GetCurrentRoundAsync()
    {
        var today = DateTime.Today;
        
        return await _dbContext.Rounds
            .FirstOrDefaultAsync(r => r.IsActive && 
                                     r.StartDate <= today && 
                                     r.EndDate >= today);
    }

    public async Task<bool> IsRoundActiveAsync(int roundId)
    {
        var round = await _dbContext.Rounds.FindAsync(roundId);
        if (round == null) return false;

        var today = DateTime.Today;
        return round.IsActive && !round.IsCompleted && 
               round.StartDate <= today && round.EndDate >= today;
    }

    public async Task<int> GetParticipantRoundScoreAsync(int participantId, int roundId)
    {
        var round = await _dbContext.Rounds.FindAsync(roundId);
        if (round == null) return 0;

        return await _scoringService.GetParticipantTotalScoreAsync(
            participantId, round.StartDate, round.EndDate);
    }

    public async Task<List<Participant>> GetEligibleParticipantsAsync(int roundId)
    {
        var round = await _dbContext.Rounds.FindAsync(roundId);
        if (round == null) return new List<Participant>();

        var participants = await _dbContext.Participants
            .Include(p => p.AgeGroup)
            .ToListAsync();

        var eligibleParticipants = new List<Participant>();

        foreach (var participant in participants)
        {
            var score = await GetParticipantRoundScoreAsync(participant.Id, roundId);
            if (score >= 195)
            {
                eligibleParticipants.Add(participant);
            }
        }

        return eligibleParticipants;
    }

    public async Task AutoCompleteExpiredRoundsAsync()
    {
        var expiredRounds = await _dbContext.Rounds
            .Where(r => r.IsActive && !r.IsCompleted && r.EndDate < DateTime.Today)
            .ToListAsync();

        foreach (var round in expiredRounds)
        {
            _logger.LogInformation("Auto-completing expired round {RoundName}", round.Name);
            await ProcessRoundWinnersAsync(round.Id);
        }
    }
} 
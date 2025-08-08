using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Data;
using SalahStreakApp.Models;

namespace SalahStreakApp.Services;

public class AttendanceScoringService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AttendanceScoringService> _logger;

    public AttendanceScoringService(ApplicationDbContext dbContext, ILogger<AttendanceScoringService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ProcessAttendanceScoresAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            _logger.LogInformation("Starting attendance score processing from {FromDate} to {ToDate}", fromDate, toDate);

            // Get active attendance calendars
            var query = _dbContext.AttendanceCalendars
                .Where(ac => ac.IsActive);

            if (fromDate.HasValue)
                query = query.Where(ac => ac.Date >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(ac => ac.Date <= toDate.Value);

            var calendars = await query.ToListAsync();

            foreach (var calendar in calendars)
            {
                await ProcessCalendarScoresAsync(calendar);
            }

            _logger.LogInformation("Completed attendance score processing");

            // Persist all score changes
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing attendance scores");
        }
    }

    private async Task ProcessCalendarScoresAsync(AttendanceCalendar calendar)
    {
        var expectedDateTime = calendar.Date.Add(calendar.ExpectedTime);
        var windowStart = expectedDateTime.AddMinutes(-calendar.TimeWindowMinutes);
        var windowEnd = expectedDateTime.AddMinutes(calendar.TimeWindowMinutes);

        // Get all participants
        var participants = await _dbContext.Participants.ToListAsync();

        foreach (var participant in participants)
        {
            await ProcessParticipantScoreAsync(participant, calendar, expectedDateTime, windowStart, windowEnd);
        }
    }

    private async Task ProcessParticipantScoreAsync(
        Participant participant, 
        AttendanceCalendar calendar, 
        DateTime expectedDateTime, 
        DateTime windowStart, 
        DateTime windowEnd)
    {
        // Get existing score record or create new one
        var existingScore = await _dbContext.AttendanceScores
            .FirstOrDefaultAsync(score => 
                score.ParticipantId == participant.Id && 
                score.AttendanceCalendarId == calendar.Id);

        if (existingScore == null)
        {
            existingScore = new AttendanceScore
            {
                ParticipantId = participant.Id,
                AttendanceCalendarId = calendar.Id,
                CreatedAt = DateTime.Now
            };
            _dbContext.AttendanceScores.Add(existingScore);
        }

        // Get biometric check-ins for this participant within the time window
        var checkIns = await _dbContext.BiometricLogs
            .Where(log => log.ParticipantId_int == participant.Id || log.ParticipantId == participant.ParticipantId)
            .Where(log => log.CheckInTime >= windowStart && log.CheckInTime <= windowEnd)
            .ToListAsync();

        // Order in-memory by closeness to expected time to avoid non-translatable SQL
        checkIns = checkIns
            .OrderBy(log => Math.Abs((log.CheckInTime - expectedDateTime).TotalMinutes))
            .ToList();

        // Get late check-ins (outside window but same day)
        var lateCheckIns = await _dbContext.BiometricLogs
            .Where(log => log.ParticipantId_int == participant.Id || log.ParticipantId == participant.ParticipantId)
            .Where(log => log.CheckInTime.Date == calendar.Date)
            .Where(log => log.CheckInTime < windowStart || log.CheckInTime > windowEnd)
            .ToListAsync();

        // Process scoring
        var (score, actualCheckIn, isLate, isDuplicate, notes) = CalculateScore(
            checkIns, lateCheckIns, expectedDateTime, windowStart, windowEnd);

        // Update score record
        existingScore.Score = score;
        existingScore.ActualCheckInTime = actualCheckIn;
        existingScore.IsLate = isLate;
        existingScore.IsDuplicate = isDuplicate;
        existingScore.Notes = notes;
        existingScore.UpdatedAt = DateTime.Now;

        // Link to biometric log if found
        if (actualCheckIn.HasValue && checkIns.Any())
        {
            var closestCheckIn = checkIns.First();
            existingScore.BiometricLogId = closestCheckIn.Id;
        }

        _logger.LogDebug("Processed score for participant {ParticipantId} on {Date}: {Score}", 
            participant.ParticipantId, calendar.Date.ToString("yyyy-MM-dd"), score);
    }

    private (int score, DateTime? actualCheckIn, bool isLate, bool isDuplicate, string? notes) 
        CalculateScore(List<BiometricLog> validCheckIns, List<BiometricLog> lateCheckIns, 
            DateTime expectedDateTime, DateTime windowStart, DateTime windowEnd)
    {
        if (!validCheckIns.Any())
        {
            // No valid check-ins
            if (lateCheckIns.Any())
            {
                return (0, lateCheckIns.First().CheckInTime, true, false, "Late check-in outside window");
            }
            return (0, null, false, false, "No check-in");
        }

        if (validCheckIns.Count == 1)
        {
            // Single valid check-in
            var checkIn = validCheckIns.First();
            var minutesFromExpected = Math.Abs((checkIn.CheckInTime - expectedDateTime).TotalMinutes);
            return (1, checkIn.CheckInTime, false, false, $"On time (±{minutesFromExpected:F0} min)");
        }

        // Multiple check-ins - find closest to expected time
        var closestCheckIn = validCheckIns.First();
        var closestMinutes = Math.Abs((closestCheckIn.CheckInTime - expectedDateTime).TotalMinutes);
        
        var isDuplicate = validCheckIns.Count > 1;
        var notes = isDuplicate 
            ? $"Multiple check-ins, using closest (±{closestMinutes:F0} min from expected)"
            : $"On time (±{closestMinutes:F0} min)";

        return (1, closestCheckIn.CheckInTime, false, isDuplicate, notes);
    }

    public async Task RecalculateScoresForWindowChangeAsync(int calendarId)
    {
        var calendar = await _dbContext.AttendanceCalendars.FindAsync(calendarId);
        if (calendar == null) return;

        _logger.LogInformation("Recalculating scores for calendar {CalendarId} due to window change", calendarId);

        // Delete existing scores for this calendar
        var existingScores = await _dbContext.AttendanceScores
            .Where(score => score.AttendanceCalendarId == calendarId)
            .ToListAsync();

        _dbContext.AttendanceScores.RemoveRange(existingScores);
        await _dbContext.SaveChangesAsync();

        // Recalculate scores
        await ProcessCalendarScoresAsync(calendar);
    }

    public async Task<int> GetParticipantTotalScoreAsync(int participantId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbContext.AttendanceScores
            .Where(score => score.ParticipantId == participantId);

        if (fromDate.HasValue)
            query = query.Where(score => score.AttendanceCalendar.Date >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(score => score.AttendanceCalendar.Date <= toDate.Value);

        return await query.SumAsync(score => score.Score);
    }

    public async Task<List<AttendanceScore>> GetParticipantScoresAsync(int participantId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbContext.AttendanceScores
            .Include(score => score.AttendanceCalendar)
            .Include(score => score.BiometricLog)
            .Where(score => score.ParticipantId == participantId);

        if (fromDate.HasValue)
            query = query.Where(score => score.AttendanceCalendar.Date >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(score => score.AttendanceCalendar.Date <= toDate.Value);

        return await query
            .OrderBy(score => score.AttendanceCalendar.Date)
            .ToListAsync();
    }
} 
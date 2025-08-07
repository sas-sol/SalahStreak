using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Data;
using SalahStreakApp.Models;
using SalahStreakApp.Services;

namespace SalahStreakApp.Controllers;

public class DemoController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DemoController> _logger;
    private readonly AttendanceScoringService _scoringService;
    private readonly RoundManagementService _roundService;
    private readonly BioTimeApiService _bioTimeService;

    public DemoController(
        ApplicationDbContext dbContext,
        ILogger<DemoController> logger,
        AttendanceScoringService scoringService,
        RoundManagementService roundService,
        BioTimeApiService bioTimeService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _scoringService = scoringService;
        _roundService = roundService;
        _bioTimeService = bioTimeService;
    }

    public async Task<IActionResult> Index()
    {
        var demoData = new DemoDashboardViewModel
        {
            TotalParticipants = await _dbContext.Participants.CountAsync(),
            TotalBiometricLogs = await _dbContext.BiometricLogs.CountAsync(),
            TotalAttendanceScores = await _dbContext.AttendanceScores.CountAsync(),
            ActiveRounds = await _roundService.GetActiveRoundsAsync(),
            CompletedRounds = await _roundService.GetCompletedRoundsAsync(),
            CurrentRound = await _roundService.GetCurrentRoundAsync(),
            RecentBiometricLogs = await _dbContext.BiometricLogs
                .Include(log => log.Participant)
                .OrderByDescending(log => log.CheckInTime)
                .Take(10)
                .ToListAsync()
        };

        return View(demoData);
    }

    [HttpPost]
    public async Task<IActionResult> GenerateTestData()
    {
        try
        {
            await GenerateComprehensiveTestDataAsync();
            await _scoringService.ProcessAttendanceScoresAsync();
            
            TempData["Success"] = "Test data generated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating test data");
            TempData["Error"] = "Error generating test data: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> ProcessRoundWinners(int roundId)
    {
        try
        {
            await _roundService.ProcessRoundWinnersAsync(roundId);
            TempData["Success"] = "Round winners processed successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing round winners");
            TempData["Error"] = "Error processing winners: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTestRound()
    {
        try
        {
            var startDate = DateTime.Today.AddDays(-30); // 30 days ago
            var round = await _roundService.CreateRoundAsync("Test Round", startDate, 40);
            TempData["Success"] = $"Test round created: {round.Name}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test round");
            TempData["Error"] = "Error creating test round: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ForceScoreProcessing()
    {
        try
        {
            // Clear existing scores
            var existingScores = await _dbContext.AttendanceScores.ToListAsync();
            _dbContext.AttendanceScores.RemoveRange(existingScores);
            await _dbContext.SaveChangesAsync();

            // Process scores again
            await _scoringService.ProcessAttendanceScoresAsync();
            
            TempData["Success"] = "Scores reprocessed successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reprocessing scores");
            TempData["Error"] = "Error reprocessing scores: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> GenerateCalendarData()
    {
        try
        {
            await GenerateSimpleCalendarDataAsync();
            TempData["Success"] = "Calendar data generated successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating calendar data");
            TempData["Error"] = "Error generating calendar data: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ResetAllData()
    {
        try
        {
            // Clear all data except participants and age groups
            var existingScores = await _dbContext.AttendanceScores.ToListAsync();
            var existingLogs = await _dbContext.BiometricLogs.ToListAsync();
            var existingCalendars = await _dbContext.AttendanceCalendars.ToListAsync();
            var existingRounds = await _dbContext.Rounds.ToListAsync();
            var existingWinners = await _dbContext.Winners.ToListAsync();

            _dbContext.AttendanceScores.RemoveRange(existingScores);
            _dbContext.BiometricLogs.RemoveRange(existingLogs);
            _dbContext.AttendanceCalendars.RemoveRange(existingCalendars);
            _dbContext.Rounds.RemoveRange(existingRounds);
            _dbContext.Winners.RemoveRange(existingWinners);

            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "All data cleared successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting data");
            TempData["Error"] = "Error resetting data: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> GenerateComprehensiveDemoData()
    {
        try
        {
            await GenerateComprehensiveDemoDataAsync();
            await _scoringService.ProcessAttendanceScoresAsync();
            
            TempData["Success"] = "Comprehensive demo data generated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating comprehensive demo data");
            TempData["Error"] = "Error generating demo data: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> DebugScoring()
    {
        var debug = new DebugScoringViewModel();

        // Get sample data
        var participant = await _dbContext.Participants.FirstOrDefaultAsync();
        if (participant != null)
        {
            var calendar = await _dbContext.AttendanceCalendars.FirstOrDefaultAsync();
            if (calendar != null)
            {
                var biometricLogs = await _dbContext.BiometricLogs
                    .Where(log => log.ParticipantId_int == participant.Id)
                    .ToListAsync();

                debug.Participant = participant;
                debug.Calendar = calendar;
                debug.BiometricLogs = biometricLogs;

                var expectedDateTime = calendar.Date.Add(calendar.ExpectedTime);
                var windowStart = expectedDateTime.AddMinutes(-calendar.TimeWindowMinutes);
                var windowEnd = expectedDateTime.AddMinutes(calendar.TimeWindowMinutes);

                debug.ExpectedTime = expectedDateTime;
                debug.WindowStart = windowStart;
                debug.WindowEnd = windowEnd;

                // Check which logs fall within the window
                debug.ValidLogs = biometricLogs
                    .Where(log => log.CheckInTime >= windowStart && log.CheckInTime <= windowEnd)
                    .ToList();

                debug.LateLogs = biometricLogs
                    .Where(log => log.CheckInTime.Date == calendar.Date && 
                                 (log.CheckInTime < windowStart || log.CheckInTime > windowEnd))
                    .ToList();
            }
        }

        return View(debug);
    }

    public async Task<IActionResult> Diagnostic()
    {
        var diagnostic = new DiagnosticViewModel();

        // Check current round
        diagnostic.CurrentRound = await _roundService.GetCurrentRoundAsync();
        
        if (diagnostic.CurrentRound != null)
        {
            // Get participants with their scores
            var participants = await _dbContext.Participants
                .Include(p => p.AgeGroup)
                .ToListAsync();

            diagnostic.ParticipantScores = new List<ParticipantScoreInfo>();

            foreach (var participant in participants)
            {
                var score = await _roundService.GetParticipantRoundScoreAsync(participant.Id, diagnostic.CurrentRound.Id);
                var totalPossible = 200; // 5 points per day * 40 days
                var percentage = (double)score / totalPossible * 100;

                diagnostic.ParticipantScores.Add(new ParticipantScoreInfo
                {
                    Participant = participant,
                    Score = score,
                    TotalPossible = totalPossible,
                    Percentage = percentage,
                    IsEligible = score >= 195
                });
            }

            // Check attendance scores
            diagnostic.AttendanceScores = await _dbContext.AttendanceScores
                .Include(s => s.Participant)
                .Include(s => s.AttendanceCalendar)
                .Where(s => s.AttendanceCalendar.Date >= diagnostic.CurrentRound.StartDate && 
                           s.AttendanceCalendar.Date <= diagnostic.CurrentRound.EndDate)
                .OrderBy(s => s.Participant.ParticipantId)
                .ThenBy(s => s.AttendanceCalendar.Date)
                .ToListAsync();

            // Check biometric logs
            diagnostic.BiometricLogs = await _dbContext.BiometricLogs
                .Include(log => log.Participant)
                .Where(log => log.CheckInTime >= diagnostic.CurrentRound.StartDate && 
                             log.CheckInTime <= diagnostic.CurrentRound.EndDate)
                .OrderBy(log => log.Participant.ParticipantId)
                .ThenBy(log => log.CheckInTime)
                .ToListAsync();
        }

        return View(diagnostic);
    }

    public async Task<IActionResult> ParticipantScores(int participantId)
    {
        var participant = await _dbContext.Participants
            .Include(p => p.AgeGroup)
            .FirstOrDefaultAsync(p => p.Id == participantId);

        if (participant == null)
        {
            return NotFound();
        }

        var scores = await _scoringService.GetParticipantScoresAsync(participantId);
        var totalScore = await _scoringService.GetParticipantTotalScoreAsync(participantId);

        var viewModel = new ParticipantScoresViewModel
        {
            Participant = participant,
            Scores = scores,
            TotalScore = totalScore
        };

        return View(viewModel);
    }

    public async Task<IActionResult> RoundDetails(int roundId)
    {
        var round = await _dbContext.Rounds
            .Include(r => r.Winners)
            .ThenInclude(w => w.Participant)
            .Include(r => r.Winners)
            .ThenInclude(w => w.AgeGroup)
            .FirstOrDefaultAsync(r => r.Id == roundId);

        if (round == null)
        {
            return NotFound();
        }

        var eligibleParticipants = await _roundService.GetEligibleParticipantsAsync(roundId);
        var winners = await _roundService.GetRoundWinnersAsync(roundId);

        var viewModel = new RoundDetailsViewModel
        {
            Round = round,
            EligibleParticipants = eligibleParticipants,
            Winners = winners
        };

        return View(viewModel);
    }

    public async Task<IActionResult> TestJwtToken()
    {
        try
        {
            var token = await _bioTimeService.GetJwtTokenAsync();
            if (token != null)
            {
                _logger.LogInformation("Successfully retrieved JWT token: {Token}", token.Substring(0, Math.Min(20, token.Length)) + "...");
                return Content($"JWT Token retrieved successfully! Token starts with: {token.Substring(0, Math.Min(20, token.Length))}...", "text/plain");
            }
            else
            {
                _logger.LogWarning("Failed to retrieve JWT token");
                return Content("Failed to retrieve JWT token. Check logs for details.", "text/plain");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing JWT token retrieval");
            return Content($"Error: {ex.Message}", "text/plain");
        }
    }

    public async Task<IActionResult> ImportBioTimeTransactions(string? empCode = null, DateTime? startTime = null, DateTime? endTime = null)
    {
        try
        {
            var imported = await _bioTimeService.ImportTransactionsAsync(startTime, endTime, empCode);
            return Content($"Imported {imported} BioTime transactions.", "text/plain");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing BioTime transactions");
            return Content($"Error: {ex.Message}", "text/plain");
        }
    }

    private async Task GenerateSimpleCalendarDataAsync()
    {
        try
        {
            // Clear existing calendar data
            var existingCalendars = await _dbContext.AttendanceCalendars.ToListAsync();
            _dbContext.AttendanceCalendars.RemoveRange(existingCalendars);
            await _dbContext.SaveChangesAsync();

            // Generate simple calendar entries for the past 30 days
            var calendarEntries = new List<AttendanceCalendar>();
            var startDate = DateTime.Today.AddDays(-30);
            var endDate = DateTime.Today;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Create 5 prayer times per day
                var fajr = new AttendanceCalendar
                {
                    Date = date,
                    ExpectedTime = new TimeSpan(5, 15, 0), // 05:15
                    TimeWindowMinutes = 30,
                    IsActive = true,
                    Description = $"Fajr on {date:yyyy-MM-dd}"
                };
                calendarEntries.Add(fajr);

                var dhuhr = new AttendanceCalendar
                {
                    Date = date,
                    ExpectedTime = new TimeSpan(12, 30, 0), // 12:30
                    TimeWindowMinutes = 30,
                    IsActive = true,
                    Description = $"Dhuhr on {date:yyyy-MM-dd}"
                };
                calendarEntries.Add(dhuhr);

                var asr = new AttendanceCalendar
                {
                    Date = date,
                    ExpectedTime = new TimeSpan(16, 0, 0), // 16:00
                    TimeWindowMinutes = 30,
                    IsActive = true,
                    Description = $"Asr on {date:yyyy-MM-dd}"
                };
                calendarEntries.Add(asr);

                var maghrib = new AttendanceCalendar
                {
                    Date = date,
                    ExpectedTime = new TimeSpan(18, 30, 0), // 18:30
                    TimeWindowMinutes = 30,
                    IsActive = true,
                    Description = $"Maghrib on {date:yyyy-MM-dd}"
                };
                calendarEntries.Add(maghrib);

                var isha = new AttendanceCalendar
                {
                    Date = date,
                    ExpectedTime = new TimeSpan(20, 0, 0), // 20:00
                    TimeWindowMinutes = 30,
                    IsActive = true,
                    Description = $"Isha on {date:yyyy-MM-dd}"
                };
                calendarEntries.Add(isha);
            }

            await _dbContext.AttendanceCalendars.AddRangeAsync(calendarEntries);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Generated {Count} calendar entries from {StartDate} to {EndDate}", 
                calendarEntries.Count, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GenerateSimpleCalendarDataAsync");
            throw;
        }
    }

    private async Task GenerateComprehensiveTestDataAsync()
    {
        try
        {
            var participants = await _dbContext.Participants.ToListAsync();
            var calendars = await _dbContext.AttendanceCalendars
                .Where(ac => ac.IsActive && ac.Date >= DateTime.Today.AddDays(-30))
                .ToListAsync();

            if (!participants.Any())
            {
                _logger.LogWarning("No participants found");
                return;
            }

            if (!calendars.Any())
            {
                _logger.LogWarning("No calendar entries found");
                return;
            }

            _logger.LogInformation("Found {ParticipantCount} participants and {CalendarCount} calendar entries", 
                participants.Count, calendars.Count);

            var random = new Random();
            var newLogs = new List<BiometricLog>();

            foreach (var participant in participants)
            {
                _logger.LogInformation("Generating logs for participant {ParticipantName} (ID: {ParticipantId})", 
                    participant.FullName, participant.ParticipantId);

                // Generate check-ins for each participant
                foreach (var calendar in calendars)
                {
                    var expectedTime = calendar.Date.Add(calendar.ExpectedTime);
                    var windowStart = expectedTime.AddMinutes(-calendar.TimeWindowMinutes);
                    var windowEnd = expectedTime.AddMinutes(calendar.TimeWindowMinutes);

                    // Different attendance patterns for different participants
                    var attendanceRate = participant.Id switch
                    {
                        1 => 0.95, // Ahmed - High attendance (95%)
                        2 => 0.85, // Fatima - Good attendance (85%)
                        3 => 0.75, // Muhammad - Moderate attendance (75%)
                        4 => 0.90, // Aisha - Very high attendance (90%)
                        5 => 0.80, // Omar - Good attendance (80%)
                        _ => 0.70  // Default
                    };

                    // Check if participant should check in (based on attendance rate)
                    if (random.NextDouble() < attendanceRate)
                    {
                        // Random time within window
                        var windowDuration = (int)(windowEnd - windowStart).TotalMinutes;
                        var randomMinutes = random.Next(0, Math.Max(1, windowDuration));
                        var checkInTime = windowStart.AddMinutes(randomMinutes);

                        var log = new BiometricLog
                        {
                            ParticipantId = participant.ParticipantId,
                            ParticipantId_int = participant.Id,
                            CheckInTime = checkInTime,
                            DeviceId = $"DEVICE_{random.Next(1, 4)}",
                            RawData = $"{{\"mock\": true, \"participant\": \"{participant.ParticipantId}\", \"attendance_rate\": {attendanceRate}}}",
                            IsProcessed = false,
                            CreatedAt = DateTime.Now
                        };

                        newLogs.Add(log);
                        _logger.LogDebug("Created log for {ParticipantName} at {CheckInTime} for {CalendarDate}", 
                            participant.FullName, checkInTime.ToString("yyyy-MM-dd HH:mm"), calendar.Date.ToString("yyyy-MM-dd"));
                    }
                }
            }

            // Clear existing biometric logs first
            var existingLogs = await _dbContext.BiometricLogs.ToListAsync();
            _dbContext.BiometricLogs.RemoveRange(existingLogs);
            await _dbContext.SaveChangesAsync();

            // Add new comprehensive test data
            await _dbContext.BiometricLogs.AddRangeAsync(newLogs);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Generated {Count} comprehensive mock biometric logs", newLogs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GenerateComprehensiveTestDataAsync");
            throw;
        }
    }

    private async Task GenerateComprehensiveDemoDataAsync()
    {
        try
        {
            // Clear existing data
            await ClearExistingDataAsync();

            // Generate 40 days of calendar entries
            await GenerateFortyDayCalendarAsync();

            // Get age groups and participants
            var ageGroups = await _dbContext.AgeGroups.ToListAsync();
            var participants = await _dbContext.Participants
                .Include(p => p.AgeGroup)
                .ToListAsync();

            if (!ageGroups.Any() || !participants.Any())
            {
                _logger.LogWarning("No age groups or participants found");
                return;
            }

            // Generate biometric logs with specific scoring patterns
            await GenerateTargetedBiometricLogsAsync(participants, ageGroups);

            _logger.LogInformation("Comprehensive demo data generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GenerateComprehensiveDemoDataAsync");
            throw;
        }
    }

    private async Task ClearExistingDataAsync()
    {
        var existingScores = await _dbContext.AttendanceScores.ToListAsync();
        var existingLogs = await _dbContext.BiometricLogs.ToListAsync();
        var existingCalendars = await _dbContext.AttendanceCalendars.ToListAsync();
        var existingRounds = await _dbContext.Rounds.ToListAsync();
        var existingWinners = await _dbContext.Winners.ToListAsync();

        _dbContext.AttendanceScores.RemoveRange(existingScores);
        _dbContext.BiometricLogs.RemoveRange(existingLogs);
        _dbContext.AttendanceCalendars.RemoveRange(existingCalendars);
        _dbContext.Rounds.RemoveRange(existingRounds);
        _dbContext.Winners.RemoveRange(existingWinners);

        await _dbContext.SaveChangesAsync();
    }

    private async Task GenerateFortyDayCalendarAsync()
    {
        var calendarEntries = new List<AttendanceCalendar>();
        var startDate = DateTime.Today.AddDays(-39); // Last 40 days
        var endDate = DateTime.Today;

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            // Create 5 prayer times per day with 20-minute windows
            var prayerTimes = new[]
            {
                new { Time = new TimeSpan(5, 15, 0), Name = "Fajr" },
                new { Time = new TimeSpan(12, 30, 0), Name = "Dhuhr" },
                new { Time = new TimeSpan(16, 0, 0), Name = "Asr" },
                new { Time = new TimeSpan(18, 30, 0), Name = "Maghrib" },
                new { Time = new TimeSpan(20, 0, 0), Name = "Isha" }
            };

            foreach (var prayer in prayerTimes)
            {
                var calendar = new AttendanceCalendar
                {
                    Date = date,
                    ExpectedTime = prayer.Time,
                    TimeWindowMinutes = 20, // 20-minute window as requested
                    IsActive = true,
                    Description = $"{prayer.Name} on {date:yyyy-MM-dd}"
                };
                calendarEntries.Add(calendar);
            }
        }

        await _dbContext.AttendanceCalendars.AddRangeAsync(calendarEntries);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Generated {Count} calendar entries for 40 days", calendarEntries.Count);
    }

    private async Task GenerateTargetedBiometricLogsAsync(List<Participant> participants, List<AgeGroup> ageGroups)
    {
        var newLogs = new List<BiometricLog>();
        var random = new Random(42); // Fixed seed for reproducible results

        _logger.LogInformation("Starting targeted biometric log generation for {ParticipantCount} participants across {AgeGroupCount} age groups", 
            participants.Count, ageGroups.Count);

        // Group participants by age group
        var participantsByAgeGroup = participants
            .GroupBy(p => p.AgeGroupId)
            .ToDictionary(g => g.Key, g => g.ToList());

        _logger.LogInformation("Participants grouped by age group: {GroupCount} groups", participantsByAgeGroup.Count);

        foreach (var ageGroup in ageGroups)
        {
            if (!participantsByAgeGroup.ContainsKey(ageGroup.Id))
            {
                _logger.LogWarning("No participants found for age group {AgeGroupName}", ageGroup.Name);
                continue;
            }

            var ageGroupParticipants = participantsByAgeGroup[ageGroup.Id];
            if (ageGroupParticipants.Count < 2)
            {
                _logger.LogWarning("Not enough participants ({Count}) in age group {AgeGroupName} for demo", 
                    ageGroupParticipants.Count, ageGroup.Name);
                continue;
            }

            // Select two participants for this age group
            var participant1 = ageGroupParticipants[0]; // Winner
            var participant2 = ageGroupParticipants[1]; // Near miss

            _logger.LogInformation("Generating logs for age group {AgeGroupName}: {Participant1} (winner) and {Participant2} (near miss)", 
                ageGroup.Name, participant1.FullName, participant2.FullName);

            // Get calendar entries for the last 40 days
            var calendars = await _dbContext.AttendanceCalendars
                .Where(ac => ac.IsActive && ac.Date >= DateTime.Today.AddDays(-39))
                .OrderBy(ac => ac.Date)
                .ThenBy(ac => ac.ExpectedTime)
                .ToListAsync();

            _logger.LogInformation("Found {CalendarCount} calendar entries for age group {AgeGroupName}", 
                calendars.Count, ageGroup.Name);

            // Generate logs for participant 1 (winner - 195+ points)
            var logs1 = GenerateParticipantLogs(participant1, calendars, 197, random); // 197 points
            newLogs.AddRange(logs1);

            // Generate logs for participant 2 (near miss - 190-194 points)
            var logs2 = GenerateParticipantLogs(participant2, calendars, 193, random); // 193 points
            newLogs.AddRange(logs2);
        }

        _logger.LogInformation("Adding {LogCount} biometric logs to database", newLogs.Count);
        await _dbContext.BiometricLogs.AddRangeAsync(newLogs);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Successfully generated {Count} targeted biometric logs", newLogs.Count);
    }

    private List<BiometricLog> GenerateParticipantLogs(Participant participant, List<AttendanceCalendar> calendars, int targetScore, Random random)
    {
        var logs = new List<BiometricLog>();
        var totalPossible = calendars.Count; // 200 possible points (40 days * 5 prayers)
        var targetAttendanceRate = (double)targetScore / totalPossible;

        _logger.LogInformation("Generating logs for {ParticipantName}: target score {TargetScore}, attendance rate {AttendanceRate:F2}%", 
            participant.FullName, targetScore, targetAttendanceRate * 100);

        // Create a list of all calendar indices and shuffle them to ensure random distribution
        var calendarIndices = Enumerable.Range(0, calendars.Count).ToList();
        var shuffledIndices = calendarIndices.OrderBy(x => random.Next()).ToList();

        // Select the first targetScore indices to ensure exact target score
        var selectedIndices = shuffledIndices.Take(targetScore).ToHashSet();

        for (int i = 0; i < calendars.Count; i++)
        {
            var calendar = calendars[i];
            var expectedDateTime = calendar.Date.Add(calendar.ExpectedTime);
            var windowStart = expectedDateTime.AddMinutes(-calendar.TimeWindowMinutes);
            var windowEnd = expectedDateTime.AddMinutes(calendar.TimeWindowMinutes);

            // Check if this calendar should have a check-in
            var shouldCheckIn = selectedIndices.Contains(i);

            if (shouldCheckIn)
            {
                // Generate check-in time within the 20-minute window
                var windowDuration = (int)(windowEnd - windowStart).TotalMinutes;
                var randomMinutes = random.Next(0, Math.Max(1, windowDuration));
                var checkInTime = windowStart.AddMinutes(randomMinutes);

                var log = new BiometricLog
                {
                    ParticipantId = participant.ParticipantId,
                    ParticipantId_int = participant.Id,
                    CheckInTime = checkInTime,
                    DeviceId = $"DEVICE_{random.Next(1, 4)}",
                    RawData = $"{{\"mock\": true, \"participant\": \"{participant.ParticipantId}\", \"target_score\": {targetScore}}}",
                    IsProcessed = false,
                    CreatedAt = DateTime.Now
                };

                logs.Add(log);
            }
        }

        _logger.LogInformation("Generated {LogCount} logs for {ParticipantName} (target: {TargetScore})", 
            logs.Count, participant.FullName, targetScore);

        return logs;
    }
}
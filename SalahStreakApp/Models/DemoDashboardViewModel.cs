namespace SalahStreakApp.Models;

public class DemoDashboardViewModel
{
    public int TotalParticipants { get; set; }
    public int TotalBiometricLogs { get; set; }
    public int TotalAttendanceScores { get; set; }
    public List<Round> ActiveRounds { get; set; } = new();
    public List<Round> CompletedRounds { get; set; } = new();
    public Round? CurrentRound { get; set; }
    public List<BiometricLog> RecentBiometricLogs { get; set; } = new();
}

public class ParticipantScoresViewModel
{
    public Participant Participant { get; set; } = null!;
    public List<AttendanceScore> Scores { get; set; } = new();
    public int TotalScore { get; set; }
}

public class RoundDetailsViewModel
{
    public Round Round { get; set; } = null!;
    public List<Participant> EligibleParticipants { get; set; } = new();
    public List<Winner> Winners { get; set; } = new();
}

public class DiagnosticViewModel
{
    public Round? CurrentRound { get; set; }
    public List<ParticipantScoreInfo> ParticipantScores { get; set; } = new();
    public List<AttendanceScore> AttendanceScores { get; set; } = new();
    public List<BiometricLog> BiometricLogs { get; set; } = new();
}

public class ParticipantScoreInfo
{
    public Participant Participant { get; set; } = null!;
    public int Score { get; set; }
    public int TotalPossible { get; set; }
    public double Percentage { get; set; }
    public bool IsEligible { get; set; }
}

public class DebugScoringViewModel
{
    public Participant? Participant { get; set; }
    public AttendanceCalendar? Calendar { get; set; }
    public List<BiometricLog> BiometricLogs { get; set; } = new();
    public DateTime? ExpectedTime { get; set; }
    public DateTime? WindowStart { get; set; }
    public DateTime? WindowEnd { get; set; }
    public List<BiometricLog> ValidLogs { get; set; } = new();
    public List<BiometricLog> LateLogs { get; set; } = new();
}

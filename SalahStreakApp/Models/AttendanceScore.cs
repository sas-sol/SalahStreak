using System.ComponentModel.DataAnnotations;

namespace SalahStreakApp.Models;

public class AttendanceScore
{
    public int Id { get; set; }
    
    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;
    
    public int AttendanceCalendarId { get; set; }
    public AttendanceCalendar AttendanceCalendar { get; set; } = null!;
    
    public int Score { get; set; } = 0; // 0 or 1
    
    public DateTime? ActualCheckInTime { get; set; }
    public int? BiometricLogId { get; set; }
    public BiometricLog? BiometricLog { get; set; }
    
    public bool IsLate { get; set; } = false;
    public bool IsDuplicate { get; set; } = false;
    
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
} 
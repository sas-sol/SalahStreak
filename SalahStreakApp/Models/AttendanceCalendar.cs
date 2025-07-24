using System.ComponentModel.DataAnnotations;

namespace SalahStreakApp.Models;

public class AttendanceCalendar
{
    public int Id { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public TimeSpan ExpectedTime { get; set; } // 24-hour format (e.g., 05:15, 21:30)
    
    [Required]
    public int TimeWindowMinutes { get; set; } = 30; // ±30 minutes default
    
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<AttendanceScore> AttendanceScores { get; set; } = new List<AttendanceScore>();
} 
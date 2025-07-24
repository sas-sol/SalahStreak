using System.ComponentModel.DataAnnotations;

namespace SalahStreakApp.Models;

public class Participant
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 120)]
    public int Age { get; set; }
    
    [Required]
    public string Gender { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string ParticipantId { get; set; } = string.Empty; // Auto-generated unique ID
    
    [Required]
    [StringLength(100)]
    public string ParentName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(15)]
    public string ParentCNIC { get; set; } = string.Empty;
    
    public int AgeGroupId { get; set; }
    public AgeGroup AgeGroup { get; set; } = null!;
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<BiometricLog> BiometricLogs { get; set; } = new List<BiometricLog>();
    public ICollection<AttendanceScore> AttendanceScores { get; set; } = new List<AttendanceScore>();
} 
using System.ComponentModel.DataAnnotations;

namespace SalahStreakApp.Models;

public class BiometricLog
{
    public int Id { get; set; }
    
    [Required]
    public string ParticipantId { get; set; } = string.Empty;
    
    public int? ParticipantId_int { get; set; } // For matching with registered participants
    public Participant? Participant { get; set; }
    
    [Required]
    public DateTime CheckInTime { get; set; }
    
    [Required]
    [StringLength(50)]
    public string DeviceId { get; set; } = string.Empty;
    
    public string? RawData { get; set; } // Store any additional raw data from API
    
    public bool IsProcessed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
} 
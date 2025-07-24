using System.ComponentModel.DataAnnotations;

namespace SalahStreakApp.Models;

public class AgeGroup
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public int MinAge { get; set; }
    
    [Required]
    public int MaxAge { get; set; }
    
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public ICollection<Reward> Rewards { get; set; } = new List<Reward>();
} 
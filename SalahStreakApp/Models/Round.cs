using System.ComponentModel.DataAnnotations;

namespace SalahStreakApp.Models;

public class Round
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public int DurationDays { get; set; } = 40; // Configurable, default 40
    
    public bool IsActive { get; set; } = true;
    public bool IsCompleted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Winner> Winners { get; set; } = new List<Winner>();
} 
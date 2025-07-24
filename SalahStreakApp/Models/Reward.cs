using System.ComponentModel.DataAnnotations;

namespace SalahStreakApp.Models;

public class Reward
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public int Quantity { get; set; } = 1;
    
    public int AgeGroupId { get; set; }
    public AgeGroup AgeGroup { get; set; } = null!;
    
    public string DeliveryStatus { get; set; } = "Pending"; // Pending, Sent
    
    public DateTime? DeliveredAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
} 
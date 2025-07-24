using System.ComponentModel.DataAnnotations;

namespace SalahStreakApp.Models;

public class Winner
{
    public int Id { get; set; }
    
    public int RoundId { get; set; }
    public Round Round { get; set; } = null!;
    
    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;
    
    public int AgeGroupId { get; set; }
    public AgeGroup AgeGroup { get; set; } = null!;
    
    public int FinalScore { get; set; }
    public int RankInAgeGroup { get; set; }
    
    public bool IsRewarded { get; set; } = false;
    public DateTime? RewardedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
} 
using System.ComponentModel.DataAnnotations;

namespace SalahStreakApp.Models;

public class BioTimeTransaction
{
    public int Id { get; set; }

    // Unique id from BioTime API response
    [Required]
    public long RemoteId { get; set; }

    public int? Emp { get; set; }
    public string? EmpCode { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }

    [Required]
    public DateTime PunchTime { get; set; }

    public string? PunchState { get; set; }
    public string? PunchStateDisplay { get; set; }
    public int? VerifyType { get; set; }
    public string? VerifyTypeDisplay { get; set; }
    public string? WorkCode { get; set; }
    public string? GpsLocation { get; set; }
    public string? AreaAlias { get; set; }
    public string? TerminalSn { get; set; }
    public double? Temperature { get; set; }
    public string? IsMask { get; set; }
    public string? TerminalAlias { get; set; }
    public DateTime? UploadTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}



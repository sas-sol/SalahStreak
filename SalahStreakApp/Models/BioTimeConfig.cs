namespace SalahStreakApp.Models;

public class BioTimeConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int PollingIntervalMinutes { get; set; } = 5;
    public string[] DeviceIds { get; set; } = Array.Empty<string>();
} 
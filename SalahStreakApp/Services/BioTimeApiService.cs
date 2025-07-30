using System.Text.Json;
using SalahStreakApp.Models;

namespace SalahStreakApp.Services;

public class BioTimeApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BioTimeApiService> _logger;
    private readonly IConfiguration _configuration;

    public BioTimeApiService(HttpClient httpClient, ILogger<BioTimeApiService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        
        // Configure basic auth
        var username = _configuration["BioTimeApi:Username"];
        var password = _configuration["BioTimeApi:Password"];
        var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{username}:{password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<List<BiometricLog>> FetchCheckInsAsync(DateTime? since = null)
    {
        try
        {
            var baseUrl = _configuration["BioTimeApi:BaseUrl"];
            var deviceIds = _configuration.GetSection("BioTimeApi:DeviceIds").Get<string[]>() ?? new string[0];
            
            var checkIns = new List<BiometricLog>();
            
            foreach (var deviceId in deviceIds)
            {
                // BioTime API endpoint for device logs
                var url = $"{baseUrl}/api/device/{deviceId}/logs";
                if (since.HasValue)
                {
                    url += $"?since={since.Value:yyyy-MM-ddTHH:mm:ss}";
                }

                _logger.LogInformation("Fetching check-ins from device {DeviceId}", deviceId);
                
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var deviceLogs = ParseBioTimeResponse(content, deviceId);
                    checkIns.AddRange(deviceLogs);
                }
                else
                {
                    _logger.LogWarning("Failed to fetch from device {DeviceId}: {StatusCode}", deviceId, response.StatusCode);
                }
            }

            _logger.LogInformation("Fetched {Count} check-ins from BioTime API", checkIns.Count);
            return checkIns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching check-ins from BioTime API");
            return new List<BiometricLog>();
        }
    }

    private List<BiometricLog> ParseBioTimeResponse(string jsonResponse, string deviceId)
    {
        try
        {
            // Mock parsing - replace with actual BioTime API response format
            var checkIns = new List<BiometricLog>();
            
            // For now, return mock data
            var random = new Random();
            var now = DateTime.Now;
            
            for (int i = 0; i < random.Next(1, 5); i++)
            {
                checkIns.Add(new BiometricLog
                {
                    ParticipantId = $"P{random.Next(1000, 9999)}",
                    CheckInTime = now.AddMinutes(-random.Next(0, 60)),
                    DeviceId = deviceId,
                    RawData = jsonResponse,
                    IsProcessed = false,
                    CreatedAt = DateTime.Now
                });
            }
            
            return checkIns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing BioTime response");
            return new List<BiometricLog>();
        }
    }
}

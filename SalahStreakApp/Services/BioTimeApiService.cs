using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Data;
using SalahStreakApp.Models;

namespace SalahStreakApp.Services;

public class BioTimeApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BioTimeApiService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _dbContext;

    public BioTimeApiService(HttpClient httpClient, ILogger<BioTimeApiService> logger, IConfiguration configuration, ApplicationDbContext dbContext)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _dbContext = dbContext;
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

    public async Task<string?> GetJwtTokenAsync()
    {
        try
        {
            var baseUrl = _configuration["BioTimeApi:BaseUrl"]?.TrimEnd('/');
            var username = _configuration["BioTimeApi:Username"];
            var password = _configuration["BioTimeApi:Password"];
            var url = $"{baseUrl}/jwt-api-token-auth/";

            var payload = new { username, password };
            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get JWT token: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("token", out var tokenElement))
            {
                return tokenElement.GetString();
            }
            _logger.LogWarning("JWT token not found in response");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching JWT token from BioTime API");
            return null;
        }
    }

    public async Task<int> ImportTransactionsAsync(DateTime? startTime = null, DateTime? endTime = null, string? empCode = null)
    {
        var token = await GetJwtTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Cannot import transactions: JWT token is null");
            return 0;
        }

        var baseUrl = _configuration["BioTimeApi:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogWarning("BioTime BaseUrl is not configured");
            return 0;
        }

        var terminalSns = GetConfiguredTerminalSerials();
        if (terminalSns.Length == 0)
        {
            _logger.LogError("No terminal serial numbers configured. Set 'BioTimeApi:TerminalSNs' (array) or 'BioTimeApi:TerminalSN' (comma-separated).");
            return 0;
        }
        _logger.LogInformation("Using terminal_sn: {SNs}", string.Join(",", terminalSns));

        var totalImported = 0;
        var nextUrl = BuildTransactionsUrl(baseUrl, terminalSns, startTime, endTime, empCode);
        _logger.LogInformation("BioTime import starting with URL: {Url}", nextUrl);

        string? previousUrl = null;
        while (!string.IsNullOrWhiteSpace(nextUrl))
        {
            _logger.LogInformation("BioTime requesting transactions page: {Url}", nextUrl);
            if (previousUrl != null && string.Equals(previousUrl, nextUrl, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("BioTime 'next' URL is identical to previous; stopping to avoid loop: {Url}", nextUrl);
                break;
            }
            previousUrl = nextUrl;

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, nextUrl);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("JWT", token);
            httpRequest.Headers.Accept.ParseAdd("application/json");
            httpRequest.Headers.TryAddWithoutValidation("Content-Type", "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch transactions: {StatusCode}", response.StatusCode);
                break;
            }

            var json = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<BioTimeTransactionsEnvelope>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed == null)
            {
                _logger.LogWarning("Failed to parse transactions response");
                break;
            }

            if (parsed.Data != null && parsed.Data.Count > 0)
            {
                var remoteIds = parsed.Data.Select(d => (long)d.Id).ToList();
                var existingRemoteIds = await _dbContext.BioTimeTransactions
                    .Where(t => remoteIds.Contains(t.RemoteId))
                    .Select(t => t.RemoteId)
                    .ToListAsync();

                var newEntities = parsed.Data
                    .Where(d => !existingRemoteIds.Contains(d.Id))
                    .Select(MapToEntity)
                    .ToList();

                if (newEntities.Count > 0)
                {
                    await _dbContext.BioTimeTransactions.AddRangeAsync(newEntities);
                    totalImported += newEntities.Count;
                    await _dbContext.SaveChangesAsync();
                }
            }

            // Follow API-provided next URL as-is
            nextUrl = parsed.Next;
        }

        _logger.LogInformation("Imported {Count} BioTime transactions", totalImported);
        return totalImported;
    }

    private static BioTimeTransaction MapToEntity(BioTimeTransactionDto dto)
    {
        return new BioTimeTransaction
        {
            RemoteId = dto.Id,
            Emp = dto.Emp,
            EmpCode = dto.EmpCode,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Department = dto.Department,
            Position = dto.Position,
            PunchTime = ParseDate(dto.PunchTime) ?? DateTime.MinValue,
            PunchState = dto.PunchState,
            PunchStateDisplay = dto.PunchStateDisplay,
            VerifyType = dto.VerifyType,
            VerifyTypeDisplay = dto.VerifyTypeDisplay,
            WorkCode = dto.WorkCode,
            GpsLocation = dto.GpsLocation,
            AreaAlias = dto.AreaAlias,
            TerminalSn = dto.TerminalSn,
            Temperature = dto.Temperature,
            IsMask = dto.IsMask,
            TerminalAlias = dto.TerminalAlias,
            UploadTime = ParseDate(dto.UploadTime)
        };
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParse(value, out var dt)) return dt;
        var formats = new[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss.fff" };
        if (DateTime.TryParseExact(value, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out dt))
        {
            return dt;
        }
        return null;
    }

    private static string BuildTransactionsUrl(string baseUrl, string[] terminalSns, DateTime? startTime, DateTime? endTime, string? empCode)
    {
        var uri = new UriBuilder($"{baseUrl}/iclock/api/transactions/");
        var query = new List<string>();
        foreach (var sn in terminalSns)
        {
            if (!string.IsNullOrWhiteSpace(sn))
            {
                query.Add($"terminal_sn={Uri.EscapeDataString(sn)}");
            }
        }
        if (!string.IsNullOrWhiteSpace(empCode))
        {
            query.Add($"emp_code={Uri.EscapeDataString(empCode)}");
        }
        if (startTime.HasValue)
        {
            query.Add($"start_time={Uri.EscapeDataString(startTime.Value.ToString("yyyy-MM-dd HH:mm:ss"))}");
        }
        if (endTime.HasValue)
        {
            query.Add($"end_time={Uri.EscapeDataString(endTime.Value.ToString("yyyy-MM-dd HH:mm:ss"))}");
        }
        uri.Query = string.Join("&", query);
        return uri.ToString();
    }

    private sealed class BioTimeTransactionsEnvelope
    {
        [JsonPropertyName("count")] public int Count { get; set; }
        [JsonPropertyName("next")] public string? Next { get; set; }
        [JsonPropertyName("previous")] public string? Previous { get; set; }
        [JsonPropertyName("msg")] public string? Msg { get; set; }
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("data")] public List<BioTimeTransactionDto> Data { get; set; } = new();
    }

    private sealed class BioTimeTransactionDto
    {
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("emp")] public int? Emp { get; set; }
        [JsonPropertyName("emp_code")] public string? EmpCode { get; set; }
        [JsonPropertyName("first_name")] public string? FirstName { get; set; }
        [JsonPropertyName("last_name")] public string? LastName { get; set; }
        [JsonPropertyName("department")] public string? Department { get; set; }
        [JsonPropertyName("position")] public string? Position { get; set; }
        [JsonPropertyName("punch_time")] public string? PunchTime { get; set; }
        [JsonPropertyName("punch_state")] public string? PunchState { get; set; }
        [JsonPropertyName("punch_state_display")] public string? PunchStateDisplay { get; set; }
        [JsonPropertyName("verify_type")] public int? VerifyType { get; set; }
        [JsonPropertyName("verify_type_display")] public string? VerifyTypeDisplay { get; set; }
        [JsonPropertyName("work_code")] public string? WorkCode { get; set; }
        [JsonPropertyName("gps_location")] public string? GpsLocation { get; set; }
        [JsonPropertyName("area_alias")] public string? AreaAlias { get; set; }
        [JsonPropertyName("terminal_sn")] public string? TerminalSn { get; set; }
        [JsonPropertyName("temperature")] public double? Temperature { get; set; }
        [JsonPropertyName("is_mask")] public string? IsMask { get; set; }
        [JsonPropertyName("terminal_alias")] public string? TerminalAlias { get; set; }
        [JsonPropertyName("upload_time")] public string? UploadTime { get; set; }
    }

    private string[] GetConfiguredTerminalSerials()
    {
        var serials = new List<string>();

        var arr = _configuration.GetSection("BioTimeApi:TerminalSNs").Get<string[]>() ?? Array.Empty<string>();
        if (arr.Length > 0) serials.AddRange(arr);

        var single = _configuration["BioTimeApi:TerminalSN"]; // allow comma/space separated
        if (!string.IsNullOrWhiteSpace(single))
        {
            serials.AddRange(single
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        // Also accept lowercase keys if user set them in environment
        var arrLower = _configuration.GetSection("BioTimeApi:terminal_sn").Get<string[]>() ?? Array.Empty<string>();
        if (arrLower.Length > 0) serials.AddRange(arrLower);

        var singleLower = _configuration["BioTimeApi:terminal_sn"];
        if (!string.IsNullOrWhiteSpace(singleLower))
        {
            serials.AddRange(singleLower
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        return serials
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    // Not used TODO: remove this later with all the references to it.
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

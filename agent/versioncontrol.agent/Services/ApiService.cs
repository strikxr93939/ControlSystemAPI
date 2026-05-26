using System.Net.Http.Json;
using System.Text.Json;
using VersionControl.Agent.Models;

namespace VersionControl.Agent.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiService> _logger;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService(HttpClient http, ILogger<ApiService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    /// <summary>Fetches active policies from API.</summary>
    public async Task<List<PolicyDto>> GetPoliciesAsync(
        CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync(
                "api/policies?activeOnly=true", ct);

            response.EnsureSuccessStatusCode();

            var policies = await response.Content
                .ReadFromJsonAsync<List<PolicyDto>>(_jsonOpts, ct);

            _logger.LogInformation(
                "Loaded {count} active policies from API",
                policies?.Count ?? 0);

            return policies ?? new List<PolicyDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "Failed to fetch policies from API. Is the API running?");
            return new List<PolicyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching policies");
            return new List<PolicyDto>();
        }
    }

    /// <summary>Sends a violation to the API.</summary>
    public async Task<bool> SendViolationAsync(
        ViolationRequest violation,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "api/violations", violation, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Violation reported: {program} v{version} on {computer}",
                    violation.ProgramName,
                    violation.Version,
                    violation.ComputerName);
                return true;
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "API rejected violation: {status} {body}",
                response.StatusCode, body);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send violation to API");
            return false;
        }
    }
}

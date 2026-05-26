using VersionControl.Agent.Models;

namespace VersionControl.Agent.Services;

public class PolicyEvaluator
{
    private readonly ILogger<PolicyEvaluator> _logger;

    public PolicyEvaluator(ILogger<PolicyEvaluator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Find the first policy that matches the given process name.
    /// Matching is case-insensitive substring search on ProgramPattern.
    /// Also checks workshop filter and exceptions list.
    /// </summary>
    public PolicyDto? MatchPolicy(
        string processName,
        List<PolicyDto> policies,
        string? computerName = null)
    {
        var now = DateTime.UtcNow;

        return policies.FirstOrDefault(p =>
        {
            if (!p.IsActive)
                return false;

            // Time window check
            if (p.StartTime > now)
                return false;
            if (p.EndTime.HasValue && p.EndTime.Value < now)
                return false;

            // Pattern match (case-insensitive substring)
            if (!processName.Contains(
                p.ProgramPattern,
                StringComparison.OrdinalIgnoreCase))
                return false;

            // Exceptions (whitelist): comma-separated computer names
            if (!string.IsNullOrWhiteSpace(p.Exceptions)
                && computerName is not null)
            {
                var exceptions = p.Exceptions
                    .Split(',', StringSplitOptions.RemoveEmptyEntries
                              | StringSplitOptions.TrimEntries);

                if (exceptions.Any(e =>
                    e.Equals(computerName,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogDebug(
                        "Process {name} excluded by exceptions list",
                        processName);
                    return false;
                }
            }

            return true;
        });
    }

    /// <summary>
    /// Evaluates whether the detected version violates the policy.
    /// Returns (isViolation, action).
    /// </summary>
    public (bool IsViolation, string Action) Evaluate(
        string? version,
        PolicyDto policy)
    {
        // If no version info, can only flag as warning
        if (string.IsNullOrWhiteSpace(version))
        {
            return policy.BlockType >= 1
                ? (true, "NoVersionInfo")
                : (false, "");
        }

        var detected = ParseVersion(version);

        // Check minimum version requirement
        if (!string.IsNullOrWhiteSpace(policy.MinVersion))
        {
            var minVer = ParseVersion(policy.MinVersion);
            if (detected < minVer)
            {
                _logger.LogDebug(
                    "Version {v} < MinVersion {min}",
                    version, policy.MinVersion);

                return (true, GetAction(policy.BlockType));
            }
        }

        // Check maximum version (blocked range)
        if (!string.IsNullOrWhiteSpace(policy.MaxVersion))
        {
            var maxVer = ParseVersion(policy.MaxVersion);
            if (detected > maxVer)
            {
                _logger.LogDebug(
                    "Version {v} > MaxVersion {max}",
                    version, policy.MaxVersion);

                return (true, GetAction(policy.BlockType));
            }
        }

        return (false, "");
    }

    // ── Private helpers ────────────────────────────────────────────────

    private static Version ParseVersion(string raw)
    {
        // Strip extra segments like "1.2.3.4.5" → take first 4
        var clean = raw.Trim();
        var parts = clean.Split('.');
        if (parts.Length > 4)
            clean = string.Join(".", parts.Take(4));

        return System.Version.TryParse(clean, out var v)
            ? v
            : new System.Version(0, 0);
    }

    private static string GetAction(int blockType) => blockType switch
    {
        0 => "Warned",
        1 => "SoftBlock",
        2 => "Killed",
        3 => "TimedBlock",
        _ => "Detected"
    };
}

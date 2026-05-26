using System.Diagnostics;
using VersionControl.Agent.Models;
using VersionControl.Agent.Services;

namespace VersionControl.Agent;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ApiService      _api;
    private readonly ProcessScanner  _scanner;
    private readonly VersionDetector _versionDetector;
    private readonly PolicyEvaluator _evaluator;

    // How often to scan (ms)
    private const int ScanIntervalMs = 10_000;  // 10 seconds

    // Deduplicate violations: don't re-report same program within this window
    private readonly Dictionary<string, DateTime> _recentViolations = new();
    private static readonly TimeSpan ViolationCooldown = TimeSpan.FromMinutes(5);

    public Worker(
        ILogger<Worker> logger,
        ApiService       api,
        ProcessScanner   scanner,
        VersionDetector  versionDetector,
        PolicyEvaluator  evaluator)
    {
        _logger          = logger;
        _api             = api;
        _scanner         = scanner;
        _versionDetector = versionDetector;
        _evaluator       = evaluator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Agent started on {machine} as {user}",
            Environment.MachineName,
            Environment.UserName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunScanCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in scan cycle");
            }

            await Task.Delay(ScanIntervalMs, stoppingToken);
        }

        _logger.LogInformation("Agent stopping.");
    }

    private async Task RunScanCycleAsync(CancellationToken ct)
    {
        _logger.LogInformation("Scan cycle started at {time}", DateTime.Now);

        // 1. Load policies from API
        var policies = await _api.GetPoliciesAsync(ct);
        if (policies.Count == 0)
        {
            _logger.LogDebug("No active policies loaded — skipping scan.");
            return;
        }

        _logger.LogInformation("{count} policies loaded", policies.Count);

        // 2. Scan running processes
        var processes = _scanner.GetUserProcesses();
        _logger.LogDebug("Scanning {count} user processes", processes.Count);

        int violationsFound = 0;

        foreach (var process in processes)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                await CheckProcessAsync(process, policies, ct);
                violationsFound++;
            }
            catch (Exception ex)
            {
                _logger.LogTrace(
                    "Error checking process {name}: {msg}",
                    process.ProcessName, ex.Message);
            }
        }

        // Cleanup old cooldown entries
        CleanupCooldownCache();

        _logger.LogInformation(
            "Scan cycle complete. Checked {count} processes.",
            processes.Count);
    }

    private async Task CheckProcessAsync(
        Process process,
        List<PolicyDto> policies,
        CancellationToken ct)
    {
        var processName = process.ProcessName;

        // Find matching policy
        var policy = _evaluator.MatchPolicy(
            processName, policies, Environment.MachineName);

        if (policy is null)
            return;

        // Get file version
        var version = _versionDetector.GetFileVersion(process);

        // Evaluate against policy rules
        var (isViolation, action) = _evaluator.Evaluate(version, policy);

        if (!isViolation)
            return;

        // Deduplicate: skip if we just reported this
        var key = $"{processName}:{policy.Id}";
        if (_recentViolations.TryGetValue(key, out var lastReported)
            && DateTime.UtcNow - lastReported < ViolationCooldown)
        {
            _logger.LogDebug(
                "Skipping duplicate violation for {name} (cooldown)",
                processName);
            return;
        }

        _logger.LogWarning(
            "VIOLATION: {process} v{version} matches policy '{pattern}' → {action}",
            processName, version ?? "unknown", policy.ProgramPattern, action);

        // Kill process if HardBlock (BlockType = 2)
        if (policy.BlockType >= 2)
        {
            TryKillProcess(process, policy);
            action = "Killed";
        }

        // Send violation to API
        var violation = new ViolationRequest
        {
            ComputerName    = Environment.MachineName,
            ProgramName     = processName,
            Version         = version ?? "",
            RequiredVersion = policy.MinVersion ?? "",
            UserAction      = action,
            UserName        = Environment.UserName,
            PolicyId        = policy.Id.ToString(),
            Message         = policy.Message,
            BlockType       = policy.BlockType
        };

        var sent = await _api.SendViolationAsync(violation, ct);
        if (sent)
        {
            _recentViolations[key] = DateTime.UtcNow;
        }
    }

    private void TryKillProcess(Process process, PolicyDto policy)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                _logger.LogWarning(
                    "Killed process {name} (PID={pid}) per policy '{pattern}'",
                    process.ProcessName, process.Id, policy.ProgramPattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to kill process {name} (PID={pid})",
                process.ProcessName, process.Id);
        }
    }

    private void CleanupCooldownCache()
    {
        var cutoff = DateTime.UtcNow - ViolationCooldown;
        var expired = _recentViolations
            .Where(kv => kv.Value < cutoff)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in expired)
            _recentViolations.Remove(key);
    }
}

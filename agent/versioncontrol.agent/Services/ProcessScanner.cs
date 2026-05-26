using System.Diagnostics;

namespace VersionControl.Agent.Services;

public class ProcessScanner
{
    private readonly ILogger<ProcessScanner> _logger;

    // System processes to always skip (no point checking them)
    private static readonly HashSet<string> _systemProcesses = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "system", "idle", "smss", "csrss", "wininit", "winlogon",
        "services", "lsass", "svchost", "dwm", "conhost",
        "registry", "fontdrvhost", "spoolsv", "taskhostw",
        "sihost", "ctfmon", "searchindexer", "runtimebroker",
        "applicationframehost", "shellexperiencehost",
        "startmenuexperiencehost", "searchhost"
    };

    public ProcessScanner(ILogger<ProcessScanner> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Returns running processes filtered to only user-space executables.
    /// Skips system/kernel processes that can't be inspected.
    /// </summary>
    public List<Process> GetUserProcesses()
    {
        var all = Process.GetProcesses();
        var result = new List<Process>();

        foreach (var proc in all)
        {
            try
            {
                // Skip known system processes
                if (_systemProcesses.Contains(proc.ProcessName))
                    continue;

                // Skip processes with PID 0 or 4 (System/Idle)
                if (proc.Id <= 4)
                    continue;

                result.Add(proc);
            }
            catch
            {
                // Access denied or process exited — skip
            }
        }

        _logger.LogDebug(
            "Scanned {total} processes, {user} user-space",
            all.Length, result.Count);

        return result;
    }

    /// <summary>Returns ALL running processes (including system).</summary>
    public List<Process> GetRunningProcesses() =>
        Process.GetProcesses().ToList();
}

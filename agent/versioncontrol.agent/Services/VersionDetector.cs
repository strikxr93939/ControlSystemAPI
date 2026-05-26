using System.Diagnostics;

namespace VersionControl.Agent.Services;

public class VersionDetector
{
    private readonly ILogger<VersionDetector> _logger;

    public VersionDetector(ILogger<VersionDetector> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Tries to get file version from process executable.
    /// Returns null if access is denied or no version info available.
    /// </summary>
    public string? GetFileVersion(Process process)
    {
        try
        {
            var path = process.MainModule?.FileName;
            if (string.IsNullOrEmpty(path))
                return null;

            var info = FileVersionInfo.GetVersionInfo(path);

            // Prefer FileVersion, fall back to ProductVersion
            var version = info.FileVersion ?? info.ProductVersion;

            if (string.IsNullOrWhiteSpace(version))
                return null;

            // Normalize: "1, 2, 3, 4" → "1.2.3.4"
            version = version.Replace(", ", ".").Replace(",", ".");

            return version.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogTrace(
                "Cannot read version for {name}: {msg}",
                process.ProcessName, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Gets version from a file path directly.
    /// </summary>
    public string? GetVersionFromPath(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            var info = FileVersionInfo.GetVersionInfo(filePath);
            return info.FileVersion ?? info.ProductVersion;
        }
        catch
        {
            return null;
        }
    }
}

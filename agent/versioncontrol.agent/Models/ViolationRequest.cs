namespace VersionControl.Agent.Models;

/// <summary>
/// Payload sent to POST /api/violations.
/// Must match CreateViolationRequest on the API side.
/// </summary>
public class ViolationRequest
{
    public string ComputerName { get; set; } = Environment.MachineName;

    public string ProgramName { get; set; } = "";

    public string Version { get; set; } = "";

    public string RequiredVersion { get; set; } = "";

    /// <summary>Detected | Warned | Killed</summary>
    public string UserAction { get; set; } = "Detected";

    public string UserName { get; set; } = Environment.UserName;

    public string PolicyId { get; set; } = "";

    public string Message { get; set; } = "";

    public int BlockType { get; set; }
}

namespace VersionControl.Domain.Entities;

public class Violation
{
    public Guid Id { get; set; }

    public string ComputerName { get; set; } = string.Empty;

    public string ProgramName { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string RequiredVersion { get; set; } = string.Empty;

    /// <summary>What action was taken: Detected, Warned, Killed, etc.</summary>
    public string UserAction { get; set; } = "Detected";

    public DateTime Timestamp { get; set; }

    public string UserName { get; set; } = string.Empty;

    /// <summary>Human-readable policy message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>0=Warning, 1=SoftBlock, 2=HardBlock, 3=Timed</summary>
    public int BlockType { get; set; }

    /// <summary>Optional: which policy triggered this violation.</summary>
    public string PolicyId { get; set; } = string.Empty;
}

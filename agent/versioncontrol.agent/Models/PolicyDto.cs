namespace VersionControl.Agent.Models;

/// <summary>
/// Matches PolicyResponse from the API exactly.
/// </summary>
public class PolicyDto
{
    public Guid Id { get; set; }

    public string ProgramPattern { get; set; } = "";

    /// <summary>Minimum required version (null = no minimum).</summary>
    public string? MinVersion { get; set; }

    /// <summary>Maximum allowed version (null = no maximum).</summary>
    public string? MaxVersion { get; set; }

    /// <summary>0=Warning, 1=SoftBlock, 2=HardBlock, 3=Timed</summary>
    public int BlockType { get; set; }

    public string Workshop { get; set; } = "";

    public bool IsActive { get; set; }

    public string Exceptions { get; set; } = "";

    public string Message { get; set; } = "";

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }
}

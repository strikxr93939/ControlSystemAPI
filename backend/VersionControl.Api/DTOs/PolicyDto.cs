using System.ComponentModel.DataAnnotations;

namespace VersionControl.Api.DTOs;

public class CreatePolicyRequest
{
    [Required, MinLength(1)]
    public string ProgramPattern { get; set; } = string.Empty;

    public string? MinVersion { get; set; }

    public string? MaxVersion { get; set; }

    public int BlockType { get; set; }

    public string Workshop { get; set; } = string.Empty;

    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    public DateTime? EndTime { get; set; }

    /// <summary>Comma-separated list of computer names to exclude.</summary>
    public string Exceptions { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Policy returned to Agent and Admin panel.
/// Fields match exactly what Agent's PolicyDto expects.
/// </summary>
public class PolicyResponse
{
    public Guid Id { get; set; }
    public string ProgramPattern { get; set; } = string.Empty;
    public string? MinVersion { get; set; }
    public string? MaxVersion { get; set; }
    public int BlockType { get; set; }
    public string Workshop { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Exceptions { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

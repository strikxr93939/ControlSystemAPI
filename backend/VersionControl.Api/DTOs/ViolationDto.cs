using System.ComponentModel.DataAnnotations;

namespace VersionControl.Api.DTOs;

/// <summary>
/// DTO for creating a new violation (sent by Agent).
/// </summary>
public class CreateViolationRequest
{
    [Required]
    public string ComputerName { get; set; } = string.Empty;

    [Required]
    public string ProgramName { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string RequiredVersion { get; set; } = string.Empty;

    public string UserAction { get; set; } = "Detected";

    public string UserName { get; set; } = string.Empty;

    public string PolicyId { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public int BlockType { get; set; }
}

/// <summary>
/// DTO returned to clients.
/// </summary>
public class ViolationResponse
{
    public Guid Id { get; set; }
    public string ComputerName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string RequiredVersion { get; set; } = string.Empty;
    public string UserAction { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int BlockType { get; set; }
    public DateTime Timestamp { get; set; }
}

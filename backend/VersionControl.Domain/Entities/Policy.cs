using VersionControl.Domain.Enums;

namespace VersionControl.Domain.Entities;

public class Policy
{
    public Guid Id { get; set; }

    public string ProgramPattern { get; set; } = string.Empty;

    public string? MinVersion { get; set; }

    public string? MaxVersion { get; set; }

    public BlockType BlockType { get; set; }

    public string Workshop { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string Exceptions { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
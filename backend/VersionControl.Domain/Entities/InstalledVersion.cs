namespace VersionControl.Domain.Entities;

public class InstalledVersion
{
    public Guid Id { get; set; }

    public Guid ProgramId { get; set; }

    public ProgramEntity Program { get; set; } = null!;

    public Guid ComputerId { get; set; }

    public Computer Computer { get; set; } = null!;

    public string Version { get; set; } = string.Empty;

    public DateTime InstalledAt { get; set; }

    public string Status { get; set; } = string.Empty;
}
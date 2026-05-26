namespace VersionControl.Domain.Entities;

public class ProgramEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "Production";

    public string Workshop { get; set; } = string.Empty;

    public string CurrentVersion { get; set; } = string.Empty;

    public double SizeMb { get; set; }

    public string Path { get; set; } = string.Empty;
}

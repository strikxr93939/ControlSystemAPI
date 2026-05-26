namespace VersionControl.Domain.Entities;

public class Computer
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Workshop { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;

    public bool IsOnline { get; set; }

    public DateTime LastSeen { get; set; }

    public string LastUser { get; set; } = string.Empty;

    public string OSVersion { get; set; } = string.Empty;

    public ICollection<InstalledVersion> Versions { get; set; }
        = new List<InstalledVersion>();
}
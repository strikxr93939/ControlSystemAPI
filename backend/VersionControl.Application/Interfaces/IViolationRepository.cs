using VersionControl.Domain.Entities;

namespace VersionControl.Application.Interfaces;

public interface IViolationRepository
{
    Task<IReadOnlyList<Violation>> GetAllAsync(int limit, CancellationToken ct = default);
    Task<Violation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Violation violation, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

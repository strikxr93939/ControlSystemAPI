using VersionControl.Domain.Entities;

namespace VersionControl.Application.Interfaces;

public interface IPolicyRepository
{
    Task<IReadOnlyList<Policy>> GetAllAsync(bool? activeOnly = null, CancellationToken ct = default);
    Task<Policy?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Policy policy, CancellationToken ct = default);
    Task UpdateAsync(Policy policy, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

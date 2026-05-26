using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VersionControl.Api.DTOs;
using VersionControl.Api.Hubs;
using VersionControl.Domain.Entities;
using VersionControl.Infrastructure.Persistence;

namespace VersionControl.Api.Controllers;

[ApiController]
[Route("api/policies")]
[Produces("application/json")]
public class PoliciesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<MonitoringHub> _hub;
    private readonly ILogger<PoliciesController> _logger;

    public PoliciesController(
        AppDbContext db,
        IHubContext<MonitoringHub> hub,
        ILogger<PoliciesController> logger)
    {
        _db     = db;
        _hub    = hub;
        _logger = logger;
    }

    /// <summary>Get all policies. Agent calls this to load active policies.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PolicyResponse>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? activeOnly = null,
        CancellationToken ct = default)
    {
        var query = _db.Policies.AsQueryable();

        if (activeOnly == true)
            query = query.Where(p => p.IsActive);

        var list = await query
            .OrderByDescending(p => p.StartTime)
            .Select(p => new PolicyResponse
            {
                Id             = p.Id,
                ProgramPattern = p.ProgramPattern,
                MinVersion     = p.MinVersion,
                MaxVersion     = p.MaxVersion,
                BlockType      = (int)p.BlockType,
                Workshop       = p.Workshop,
                StartTime      = p.StartTime,
                EndTime        = p.EndTime,
                Exceptions     = p.Exceptions,
                Message        = p.Message,
                IsActive       = p.IsActive
            })
            .ToListAsync(ct);

        _logger.LogDebug("GET /api/policies → {count} items", list.Count);
        return Ok(list);
    }

    /// <summary>Get single policy.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PolicyResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var p = await _db.Policies.FindAsync(new object[] { id }, ct);
        if (p is null) return NotFound();
        return Ok(MapToResponse(p));
    }

    /// <summary>Create a new policy.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PolicyResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePolicyRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var policy = new Policy
        {
            Id             = Guid.NewGuid(),
            ProgramPattern = request.ProgramPattern,
            MinVersion     = request.MinVersion,
            MaxVersion     = request.MaxVersion,
            BlockType      = (VersionControl.Domain.Enums.BlockType)request.BlockType,
            Workshop       = request.Workshop,
            StartTime      = request.StartTime,
            EndTime        = request.EndTime,
            Exceptions     = request.Exceptions,
            Message        = request.Message,
            IsActive       = request.IsActive
        };

        _db.Policies.Add(policy);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Policy created: {pattern} (BlockType={bt})",
            policy.ProgramPattern, policy.BlockType);

        var dto = MapToResponse(policy);

        // Notify admin panels that policies changed
        await _hub.Clients.All.SendAsync("PoliciesUpdated", ct);

        return CreatedAtAction(nameof(GetById),
            new { id = policy.Id }, dto);
    }

    /// <summary>Update a policy.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PolicyResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreatePolicyRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var policy = await _db.Policies.FindAsync(new object[] { id }, ct);
        if (policy is null) return NotFound();

        policy.ProgramPattern = request.ProgramPattern;
        policy.MinVersion     = request.MinVersion;
        policy.MaxVersion     = request.MaxVersion;
        policy.BlockType      = (VersionControl.Domain.Enums.BlockType)request.BlockType;
        policy.Workshop       = request.Workshop;
        policy.StartTime      = request.StartTime;
        policy.EndTime        = request.EndTime;
        policy.Exceptions     = request.Exceptions;
        policy.Message        = request.Message;
        policy.IsActive       = request.IsActive;

        await _db.SaveChangesAsync(ct);

        await _hub.Clients.All.SendAsync("PoliciesUpdated", ct);

        return Ok(MapToResponse(policy));
    }

    /// <summary>Toggle active status.</summary>
    [HttpPatch("{id:guid}/toggle")]
    [ProducesResponseType(typeof(PolicyResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct = default)
    {
        var policy = await _db.Policies.FindAsync(new object[] { id }, ct);
        if (policy is null) return NotFound();

        policy.IsActive = !policy.IsActive;
        await _db.SaveChangesAsync(ct);

        await _hub.Clients.All.SendAsync("PoliciesUpdated", ct);

        return Ok(MapToResponse(policy));
    }

    /// <summary>Delete a policy.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var policy = await _db.Policies.FindAsync(new object[] { id }, ct);
        if (policy is null) return NotFound();

        _db.Policies.Remove(policy);
        await _db.SaveChangesAsync(ct);

        await _hub.Clients.All.SendAsync("PoliciesUpdated", ct);

        return NoContent();
    }

    private static PolicyResponse MapToResponse(Policy p) => new()
    {
        Id             = p.Id,
        ProgramPattern = p.ProgramPattern,
        MinVersion     = p.MinVersion,
        MaxVersion     = p.MaxVersion,
        BlockType      = (int)p.BlockType,
        Workshop       = p.Workshop,
        StartTime      = p.StartTime,
        EndTime        = p.EndTime,
        Exceptions     = p.Exceptions,
        Message        = p.Message,
        IsActive       = p.IsActive
    };
}

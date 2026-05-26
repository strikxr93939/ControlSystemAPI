using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VersionControl.Api.DTOs;
using VersionControl.Api.Hubs;
using VersionControl.Domain.Entities;
using VersionControl.Infrastructure.Persistence;

namespace VersionControl.Api.Controllers;

[ApiController]
[Route("api/violations")]
[Produces("application/json")]
public class ViolationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<MonitoringHub> _hub;
    private readonly ILogger<ViolationsController> _logger;

    public ViolationsController(
        AppDbContext db,
        IHubContext<MonitoringHub> hub,
        ILogger<ViolationsController> logger)
    {
        _db     = db;
        _hub    = hub;
        _logger = logger;
    }

    /// <summary>Get last 100 violations, newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ViolationResponse>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int limit = 100,
        CancellationToken ct  = default)
    {
        limit = Math.Clamp(limit, 1, 500);

        var list = await _db.Violations
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .Select(v => new ViolationResponse
            {
                Id              = v.Id,
                ComputerName    = v.ComputerName,
                ProgramName     = v.ProgramName,
                Version         = v.Version,
                RequiredVersion = v.RequiredVersion,
                UserAction      = v.UserAction,
                UserName        = v.UserName,
                Message         = v.Message,
                BlockType       = v.BlockType,
                Timestamp       = v.Timestamp
            })
            .ToListAsync(ct);

        return Ok(list);
    }

    /// <summary>Get single violation by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ViolationResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var v = await _db.Violations.FindAsync(new object[] { id }, ct);
        if (v is null) return NotFound();

        return Ok(MapToResponse(v));
    }

    /// <summary>
    /// Create a new violation.
    /// Called by Agent when a policy violation is detected.
    /// Broadcasts the new violation via SignalR to all admin panels.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ViolationResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateViolationRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var violation = new Violation
        {
            Id              = Guid.NewGuid(),
            ComputerName    = request.ComputerName,
            ProgramName     = request.ProgramName,
            Version         = request.Version,
            RequiredVersion = request.RequiredVersion,
            UserAction      = request.UserAction,
            UserName        = request.UserName,
            Message         = request.Message,
            BlockType       = request.BlockType,
            PolicyId        = request.PolicyId,
            Timestamp       = DateTime.UtcNow
        };

        _db.Violations.Add(violation);
        await _db.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Violation created: {program} on {computer} (v{version})",
            violation.ProgramName,
            violation.ComputerName,
            violation.Version);

        var dto = MapToResponse(violation);

        // Broadcast realtime to admin panels
        await _hub.Clients.All.SendAsync(
            "ViolationReceived", dto, ct);

        return CreatedAtAction(nameof(GetById),
            new { id = violation.Id }, dto);
    }

    /// <summary>Delete a violation by ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var v = await _db.Violations.FindAsync(new object[] { id }, ct);
        if (v is null) return NotFound();

        _db.Violations.Remove(v);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Private helpers ────────────────────────────────────────────────
    private static ViolationResponse MapToResponse(Violation v) => new()
    {
        Id              = v.Id,
        ComputerName    = v.ComputerName,
        ProgramName     = v.ProgramName,
        Version         = v.Version,
        RequiredVersion = v.RequiredVersion,
        UserAction      = v.UserAction,
        UserName        = v.UserName,
        Message         = v.Message,
        BlockType       = v.BlockType,
        Timestamp       = v.Timestamp
    };
}

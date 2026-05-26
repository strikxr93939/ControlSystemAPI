using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VersionControl.Domain.Entities;
using VersionControl.Infrastructure.Persistence;

namespace VersionControl.Api.Controllers;

[ApiController]
[Route("api/computers")]
public class ComputersController : ControllerBase
{
    private readonly AppDbContext _db;

    public ComputersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var computers = await _db.Computers
            .OrderBy(x => x.Name)
            .ToListAsync();

        return Ok(computers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var computer = await _db.Computers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (computer == null)
            return NotFound();

        return Ok(computer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Computer computer)
    {
        computer.Id = Guid.NewGuid();

        computer.LastSeen = DateTime.UtcNow;

        _db.Computers.Add(computer);

        await _db.SaveChangesAsync();

        return Ok(computer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Computer request)
    {
        var computer = await _db.Computers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (computer == null)
            return NotFound();

        computer.Name = request.Name;
        computer.Workshop = request.Workshop;
        computer.IpAddress = request.IpAddress;
        computer.IsOnline = request.IsOnline;
        computer.LastUser = request.LastUser;
        computer.OSVersion = request.OSVersion;

        await _db.SaveChangesAsync();

        return Ok(computer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var computer = await _db.Computers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (computer == null)
            return NotFound();

        _db.Computers.Remove(computer);

        await _db.SaveChangesAsync();

        return Ok();
    }
}
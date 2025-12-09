using Microsoft.AspNetCore.Mvc;
using NonBon.Api.Models;

namespace NonBon.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FocusController : ControllerBase
{
    // In-memory storage for demo purposes.
    private static readonly List<FocusItem> Items = new();
    private static int _nextId = 1;

    // Hard limit on concurrently active focuses
    private const int MaxActive = 3;

    [HttpGet]
    public ActionResult<IEnumerable<FocusItem>> GetAll()
    {
        return Ok(Items);
    }

    [HttpGet("active")]
    public ActionResult<IEnumerable<FocusItem>> GetActive()
    {
        var active = Items.Where(i => i.Status == "Active").ToList();
        return Ok(active);
    }

    [HttpGet("backlog/random")]
    public ActionResult<FocusItem> GetRandomBacklog()
    {
        var backlog = Items.Where(i => i.Status == "Backlog").ToList();
        if (backlog.Count == 0) return NotFound();

        var random = new Random();
        var item = backlog[random.Next(backlog.Count)];
        return Ok(item);
    }

    [HttpPost]
    public ActionResult<FocusItem> Create(FocusItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Title))
        {
            ModelState.AddModelError(nameof(item.Title), "Title is required.");
            return ValidationProblem(ModelState);
        }

        if (item.Status == "Active" && CountActive() >= MaxActive)
        {
            return BadRequest($"Cannot add more than {MaxActive} active items.");
        }

        item.Id = _nextId++;
        if (string.IsNullOrWhiteSpace(item.Status))
        {
            item.Status = "Backlog";
        }

        Items.Add(item);
        return CreatedAtAction(nameof(GetAll), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}/status")]
    public ActionResult UpdateStatus(int id, [FromBody] string newStatus)
    {
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item is null) return NotFound();

        if (newStatus == "Active" && item.Status != "Active" && CountActive() >= MaxActive)
        {
            return BadRequest($"Cannot have more than {MaxActive} active items.");
        }

        item.Status = newStatus;
        return NoContent();
    }

    private static int CountActive()
    {
        return Items.Count(i => i.Status == "Active");
    }
}
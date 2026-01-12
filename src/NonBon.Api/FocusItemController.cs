using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NonBon.Api.Models;

namespace NonBon.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FocusController : ControllerBase
{
    private readonly ILogger<FocusController> _logger;

    // In-memory storage for demo purposes
    private static readonly List<FocusItem> Items = new();
    private static int _nextId = 1;

    // Hard limit on concurrently active focuses
    private const int MaxActive = 3;

    // Allowed status values; all inputs are normalized to this set
    private static readonly string[] AllowedStatuses = new[] { "Backlog", "Active", "Done", "Archived" };

    public FocusController(ILogger<FocusController> logger)
    {
        _logger = logger;
    }

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
        if (backlog.Count == 0)
        {
            _logger.LogWarning("Random backlog request failed: backlog is empty.");
            return NotFound(CreateProblem("Backlog is empty", "No backlog items are available.", StatusCodes.Status404NotFound));
        }

        var random = new Random();
        var item = backlog[random.Next(backlog.Count)];
        return Ok(item);
    }

    [HttpPost]
    public ActionResult<FocusItem> Create(FocusItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Title))
        {
            _logger.LogWarning("Create failed: title was missing.");
            ModelState.AddModelError(nameof(item.Title), "Title is required.");
            return ValidationProblem(ModelState);
        }

        // Determine desired status, defaulting and validating against the allowed set
        string desiredStatus = item.Status;
        if (string.IsNullOrWhiteSpace(desiredStatus))
        {
            desiredStatus = "Backlog";
        }
        else if (!TryNormalizeStatus(desiredStatus, out var normalizedStatus))
        {
            _logger.LogWarning("Create failed: invalid status {Status}.", desiredStatus);
            ModelState.AddModelError(nameof(item.Status), $"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
            return ValidationProblem(ModelState);
        }
        else
        {
            desiredStatus = normalizedStatus;
        }

        if (desiredStatus == "Active" && CountActive() >= MaxActive)
        {
            var detail = $"Cannot add more than {MaxActive} active items.";
            _logger.LogWarning("Create failed: {Detail}", detail);
            return BadRequest(CreateProblem("Max active limit reached", detail, StatusCodes.Status400BadRequest));
        }

        item.Id = _nextId++;
        item.Status = desiredStatus;

        Items.Add(item);
        return CreatedAtAction(nameof(GetAll), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}/status")]
    public ActionResult UpdateStatus(int id, [FromBody] string newStatus)
    {
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item is null)
        {
            _logger.LogWarning("UpdateStatus failed: item {Id} not found.", id);
            return NotFound(CreateProblem("Focus item not found", $"No focus item found with id {id}.", StatusCodes.Status404NotFound));
        }

        if (string.IsNullOrWhiteSpace(newStatus))
        {
            _logger.LogWarning("UpdateStatus failed: status was missing for item {Id}.", id);
            ModelState.AddModelError("Status", "Status is required.");
            return ValidationProblem(ModelState);
        }

        if (!TryNormalizeStatus(newStatus, out var normalizedStatus))
        {
            _logger.LogWarning("UpdateStatus failed: invalid status {Status} for item {Id}.", newStatus, id);
            ModelState.AddModelError("Status", $"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
            return ValidationProblem(ModelState);
        }

        if (normalizedStatus == "Active" && item.Status != "Active" && CountActive() >= MaxActive)
        {
            var detail = $"Cannot have more than {MaxActive} active items.";
            _logger.LogWarning("UpdateStatus failed: {Detail}", detail);
            return BadRequest(CreateProblem("Max active limit reached", detail, StatusCodes.Status400BadRequest));
        }

        item.Status = normalizedStatus;
        return NoContent();
    }

    private static int CountActive()
    {
        return Items.Count(i => i.Status == "Active");
    }

    private static bool TryNormalizeStatus(string status, out string normalized)
    {
        foreach (var allowed in AllowedStatuses)
        {
            if (string.Equals(status, allowed, StringComparison.OrdinalIgnoreCase))
            {
                normalized = allowed;
                return true;
            }
        }

        normalized = string.Empty;
        return false;
    }

    private static ProblemDetails CreateProblem(string title, string detail, int status)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status
        };
    }
}

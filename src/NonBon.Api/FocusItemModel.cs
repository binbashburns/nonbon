namespace NonBon.Api.Models;

public class FocusItem
{
    public int Id { get; set; }

    // e.g. "Finish spring cleaning"
    public string Title { get; set; } = string.Empty;

    // e.g. "Work", "Learning", "Home", "Creative"
    public string Area { get; set; } = string.Empty;

    // "Active", "Backlog", "Done" or "Archived"
    public string Status { get; set; } = "Backlog";
}
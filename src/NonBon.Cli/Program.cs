using System.Net.Http.Json;
using System.Linq;
using NonBon.Cli.Models;

var http = new HttpClient
{
    BaseAddress = new Uri("http://localhost:5000/")
};

while (true)
{
    // Basic text menu for working with focus items
    Console.WriteLine("NonBon (Not Another New Backlog)");
    Console.WriteLine("1) List active focuses");
    Console.WriteLine("2) List backlog");
    Console.WriteLine("3) Add new focus");
    Console.WriteLine("4) Change focus status");
    Console.WriteLine("5) Suggest a random backlog item");
    Console.WriteLine("0) Exit");
    Console.Write("Select an option: ");

    var choice = Console.ReadLine();
    Console.WriteLine();

    switch (choice)
    {
        case "1":
            await ListActiveAsync(http);
            break;
        case "2":
            await ListBacklogAsync(http);
            break;
        case "3":
            await AddFocusAsync(http);
            break;
        case "4":
            await ChangeStatusAsync(http);
            break;
        case "5":
            await SuggestRandomAsync(http);
            break;
        case "0":
            return;
        default:
            Console.WriteLine("Invalid option.");
            break;
    }

    Console.WriteLine();
}

// Show all items that are currently Active
static async Task ListActiveAsync(HttpClient http)
{
    var items = await http.GetFromJsonAsync<List<FocusItemDto>>("api/focus/active")
                ?? new List<FocusItemDto>();

    if (items.Count == 0)
    {
        Console.WriteLine("No active focuses.");
        return;
    }

    foreach (var item in items)
    {
        Console.WriteLine($"{item.Id}: {item.Title} [{item.Area}] ({item.Status})");
    }
}

// Show everything in the backlog (Status == "Backlog")
static async Task ListBacklogAsync(HttpClient http)
{
    var items = await http.GetFromJsonAsync<List<FocusItemDto>>("api/focus")
                ?? new List<FocusItemDto>();

    var backlog = items.Where(i => i.Status == "Backlog").ToList();

    if (backlog.Count == 0)
    {
        Console.WriteLine("Backlog is empty.");
        return;
    }

    foreach (var item in backlog)
    {
        Console.WriteLine($"{item.Id}: {item.Title} [{item.Area}] ({item.Status})");
    }
}

// Ask the user for details and send a POST to create a new focus item
static async Task AddFocusAsync(HttpClient http)
{
    Console.Write("Title: ");
    var title = Console.ReadLine() ?? "";

    Console.WriteLine("Area:");
    Console.WriteLine("1) Work");
    Console.WriteLine("2) Learning");
    Console.WriteLine("3) Home");
    Console.Write("Select an option: ");
    var areaChoice = Console.ReadLine();
    Console.WriteLine();
    var area = areaChoice switch
    {
        "1" => "Work",
        "2" => "Learning",
        "3" => "Home",
        _ => "Other"
    };

    Console.WriteLine("Status:");
    Console.WriteLine("1) Backlog");
    Console.WriteLine("2) Active");
    Console.WriteLine("3) Done");
    Console.Write("Select an option (just hit Enter for Backlog): ");
    var statusChoice = Console.ReadLine();
    Console.WriteLine();
    var status = statusChoice switch
    {
        "2" => "Active",
        "3" => "Done",
        _ => "Backlog"
    };

    var item = new FocusItemDto
    {
        Title = title,
        Area = area,
        Status = status
    };

    var response = await http.PostAsJsonAsync("api/focus", item);

    Console.WriteLine(response.IsSuccessStatusCode
        ? "Focus added."
        : $"Failed to add focus. {await response.Content.ReadAsStringAsync()}");
}

// Let the user pick a backlog item and change its status
static async Task ChangeStatusAsync(HttpClient http)
{
    var items = await http.GetFromJsonAsync<List<FocusItemDto>>("api/focus")
                ?? new List<FocusItemDto>();

    var backlog = items.Where(i => i.Status == "Backlog").ToList();

    if (backlog.Count == 0)
    {
        Console.WriteLine("Backlog is empty. Nothing to update.");
        return;
    }

    Console.WriteLine("Select an Item from the Backlog:");
    for (int i = 0; i < backlog.Count; i++)
    {
        var item = backlog[i];
        Console.WriteLine($"{i + 1}) {item.Title} [{item.Area}]");
    }

    Console.Write("Item: ");
    if (!int.TryParse(Console.ReadLine(), out var index) || index < 1 || index > backlog.Count)
    {
        Console.WriteLine("Invalid selection.");
        return;
    }

    var selected = backlog[index - 1];

    Console.WriteLine($"Select the status for \"{selected.Title}\":");
    Console.WriteLine("1) Move to Backlog");
    Console.WriteLine("2) Move to Active");
    Console.WriteLine("3) Move to Done");
    Console.Write("Select an option: ");
    var statusChoice = Console.ReadLine();

    string newStatus = statusChoice switch
    {
        "2" => "Active",
        "3" => "Done",
        _ => "Backlog"
    };

    var response = await http.PutAsJsonAsync($"api/focus/{selected.Id}/status", newStatus);

    Console.WriteLine(response.IsSuccessStatusCode
        ? "Status updated."
        : $"Failed to update status. {await response.Content.ReadAsStringAsync()}");
}

// Ask the API for a random backlog item and display it
static async Task SuggestRandomAsync(HttpClient http)
{
    var response = await http.GetAsync("api/focus/backlog/random");

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine("No backlog items to suggest.");
        return;
    }

    var item = await response.Content.ReadFromJsonAsync<FocusItemDto>();
    if (item is null)
    {
        Console.WriteLine("No suggestion available.");
        return;
    }

    Console.WriteLine("Suggested focus from backlog:");
    Console.WriteLine($"{item.Id}: {item.Title} [{item.Area}]");
}

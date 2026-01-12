using NonBon.Api.Controllers;
using NonBon.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace NonBon.Tests;

public class FocusApiTests
{
    [Fact]
    // Returns OK and a list for GET /api/focus.
    public void GetFocus_ReturnsOkAndList()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetAll();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsType<List<FocusItem>>(ok.Value);
        Assert.NotNull(items);
    }

    [Fact]
    // Creates a focus item and returns CreatedAtAction.
    public void PostFocus_CreatesNewItem()
    {
        // Arrange
        var controller = CreateController();
        var newItem = new FocusItem
        {
            Title = "Write unit tests",
            Area = "Learning",
            Status = "Backlog"
        };

        // Act
        var result = controller.Create(newItem);

        // Assert on response
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returned = Assert.IsType<FocusItem>(created.Value);
        Assert.Equal("Write unit tests", returned.Title);
        Assert.True(returned.Id > 0);

        // And verify it shows up when listing
        var listResult = controller.GetAll();
        var ok = Assert.IsType<OkObjectResult>(listResult.Result);
        var items = Assert.IsType<List<FocusItem>>(ok.Value);
        Assert.Contains(items, i => i.Id == returned.Id);
    }

    [Fact]
    // Updates status for an existing item and returns NoContent.
    public void PutStatus_ChangesStatusToDone()
    {
        // Arrange: start with a controller and one created item
        var controller = CreateController();
        var newItem = new FocusItem
        {
            Title = "Move this to done",
            Area = "Work",
            Status = "Backlog"
        };

        var createResult = controller.Create(newItem);
        var created = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        var createdItem = Assert.IsType<FocusItem>(created.Value);
        var id = createdItem.Id;

        // Act: change status
        var updateResult = controller.UpdateStatus(id, "Done");

        // Assert on the status code
        Assert.IsType<NoContentResult>(updateResult);

        // And verify in the list
        var listResult = controller.GetAll();
        var ok = Assert.IsType<OkObjectResult>(listResult.Result);
        var items = Assert.IsType<List<FocusItem>>(ok.Value);

        var updated = items.Single(i => i.Id == id);
        Assert.Equal("Done", updated.Status);
    }

    [Fact]
    // Enforces the max-active rule when creating items.
    public void PostFocus_ReturnsBadRequest_WhenMaxActiveExceeded()
    {
        // Arrange: seed the controller with MaxActive active items
        var controller = CreateController();
        for (int i = 0; i < 3; i++) // 3 == MaxActive in controller
        {
            var activeItem = new FocusItem
            {
                Title = $"Active {i + 1}",
                Area = "Test",
                Status = "Active"
            };

            var createResult = controller.Create(activeItem);
            Assert.IsType<CreatedAtActionResult>(createResult.Result);
        }

        // Act: attempt to create one more Active item
        var tooMany = new FocusItem
        {
            Title = "This should exceed the limit",
            Area = "Test",
            Status = "Active"
        };

        var result = controller.Create(tooMany);

        // Assert: returns 400 with an explanatory message
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
        Assert.Contains("active", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    private static FocusController CreateController()
    {
        return new FocusController(NullLogger<FocusController>.Instance);
    }
}

using NonBon.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Register CORS so that the browser can call this API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Register controllers for Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build the WebApplication from the configured builder
var app = builder.Build();

// Enable Swagger UI so we can explore and test endpoints in the browser
app.UseSwagger();
app.UseSwaggerUI();

// Add middleware to handle CORS and route HTTP requests to controller actions
app.UseCors();

app.MapGet("/health", () => Results.Ok("OK"));

app.MapControllers();

// Start the web server
app.Run();

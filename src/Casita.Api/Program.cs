using Casita.Api.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Create Ticket
app.MapPost("/tickets", (Ticket ticket) =>
{
    ticket = ticket with
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    return Results.Ok(ticket);
});


app.Run();

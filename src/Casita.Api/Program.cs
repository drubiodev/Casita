using Casita.Api.Features.Tickets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Registerservices(builder.Configuration);

var app = builder.Build();

TicketEndpoints.MapTicketEndpoints(app);

app.Run();

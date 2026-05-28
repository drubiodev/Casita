using Casita.Api.Features.Tickets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.Registerservices(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

TicketEndpoints.MapTicketEndpoints(app);

app.Run();

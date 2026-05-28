using Casita.Api.Features.Auth;
using Casita.Api.Features.Tickets;
using Casita.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
// Override the Infrastructure default so the DB connection factory can set
// app.user_id for RLS.
builder.Services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();

builder.Services.Registerservices(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

TicketEndpoints.MapTicketEndpoints(app);

app.Run();

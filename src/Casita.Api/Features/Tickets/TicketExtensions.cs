using Casita.Infrastructure;

namespace Casita.Api.Features.Tickets;

public static class TicketExtensions
{
    public static IServiceCollection Registerservices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddScoped<ITicketService, TicketService>();
        return services;
    }
}